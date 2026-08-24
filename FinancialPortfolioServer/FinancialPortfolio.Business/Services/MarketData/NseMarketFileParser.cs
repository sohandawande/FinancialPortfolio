using FinancialPortfolio.Business.Common.Helpers;

namespace FinancialPortfolio.Business.Services.MarketData
{
    /// <summary>
    /// Parses official NSE CSV payloads into typed rows. No HTTP, no EF.
    /// </summary>
    public static class NseMarketFileParser
    {
        public static IReadOnlyList<NseBhavcopyRow> ParseUdiff(
            string csv,
            DateOnly fallbackDate,
            string exchange,
            string[] allowedSeries)
        {
            var lines = NseCsvHelper.SplitLines(csv);
            if (lines.Length < 2)
                return [];

            var map = NseCsvHelper.HeaderMap(NseCsvHelper.SplitCsv(lines[0]));
            var allowed = NseCsvHelper.AllowedSeries(allowedSeries);
            var rows = new List<NseBhavcopyRow>(lines.Length);

            for (var i = 1; i < lines.Length; i++)
            {
                var cols = NseCsvHelper.SplitCsv(lines[i]);
                var series = NseCsvHelper.Get(cols, map, "SctySrs", "SERIES", "Series").ToUpperInvariant();
                if (allowed.Count > 0 && !allowed.Contains(series))
                    continue;

                var symbol = NseCsvHelper.Get(cols, map, "TckrSymb", "SYMBOL", "Symbol").ToUpperInvariant();
                var isin = NseCsvHelper.Get(cols, map, "ISIN", "ISINCode").ToUpperInvariant();
                if (string.IsNullOrWhiteSpace(symbol) || string.IsNullOrWhiteSpace(isin))
                    continue;

                var close = NseCsvHelper.ParseDecimal(NseCsvHelper.Get(cols, map, "ClsPric", "CLOSE", "Close"));
                if (close <= 0)
                    continue;

                rows.Add(new NseBhavcopyRow
                {
                    Exchange = exchange,
                    TradeDate = NseCsvHelper.ParseDate(NseCsvHelper.Get(cols, map, "TradDt", "TIMESTAMP")) ?? fallbackDate,
                    Symbol = symbol,
                    CompanyName = NseCsvHelper.Get(cols, map, "FinInstrmNm", "SECURITY", "CompanyName"),
                    Isin = isin,
                    Series = series,
                    Open = NseCsvHelper.ParseDecimal(NseCsvHelper.Get(cols, map, "OpnPric", "OPEN", "Open")),
                    High = NseCsvHelper.ParseDecimal(NseCsvHelper.Get(cols, map, "HghPric", "HIGH", "High")),
                    Low = NseCsvHelper.ParseDecimal(NseCsvHelper.Get(cols, map, "LwPric", "LOW", "Low")),
                    Close = close,
                    PreviousClose = NseCsvHelper.ParseDecimal(NseCsvHelper.Get(cols, map, "PrvsClsgPric", "PREVCLOSE", "PrevClose")),
                    Volume = NseCsvHelper.ParseLong(NseCsvHelper.Get(cols, map, "TtlTradgVol", "TOTTRDQTY", "Volume"))
                });
            }

            return rows;
        }

        public static IReadOnlyList<NseBhavcopyRow> ParseClassicBhav(string csv, DateOnly fallbackDate, string exchange)
        {
            var (header, start) = NseCsvHelper.FindHeader(csv, "SYMBOL", "TckrSymb");
            if (header.Count == 0)
                return [];

            var map = NseCsvHelper.HeaderMap(header);
            var lines = NseCsvHelper.SplitLines(csv);
            var rows = new List<NseBhavcopyRow>();

            for (var i = start; i < lines.Length; i++)
            {
                var cols = NseCsvHelper.SplitCsv(lines[i]);
                var symbol = NseCsvHelper.Get(cols, map, "SYMBOL", "TckrSymb", "Symbol").ToUpperInvariant();
                if (string.IsNullOrWhiteSpace(symbol))
                    continue;

                var close = NseCsvHelper.ParseDecimal(NseCsvHelper.Get(cols, map, "CLOSE", "ClsPric", "Close"));
                if (close <= 0)
                    continue;

                var series = NseCsvHelper.Get(cols, map, "SERIES", "SctySrs", "Series").ToUpperInvariant();
                if (string.IsNullOrWhiteSpace(series))
                    series = "SM";

                rows.Add(new NseBhavcopyRow
                {
                    Exchange = exchange,
                    TradeDate = fallbackDate,
                    Symbol = symbol,
                    CompanyName = NseCsvHelper.Get(cols, map, "SECURITY", "FinInstrmNm", "NAME"),
                    Isin = NseCsvHelper.Get(cols, map, "ISIN").ToUpperInvariant(),
                    Series = series,
                    Open = NseCsvHelper.ParseDecimal(NseCsvHelper.Get(cols, map, "OPEN", "OpnPric")),
                    High = NseCsvHelper.ParseDecimal(NseCsvHelper.Get(cols, map, "HIGH", "HghPric")),
                    Low = NseCsvHelper.ParseDecimal(NseCsvHelper.Get(cols, map, "LOW", "LwPric")),
                    Close = close,
                    PreviousClose = NseCsvHelper.ParseDecimal(NseCsvHelper.Get(cols, map, "PREVCLOSE", "PrvsClsgPric")),
                    Volume = NseCsvHelper.ParseLong(NseCsvHelper.Get(cols, map, "TOTTRDQTY", "TtlTradgVol"))
                });
            }

            return rows;
        }

        public static IReadOnlyList<NseMcapRow> ParseMcap(string csv)
        {
            var (header, start) = NseCsvHelper.FindHeader(csv, "Symbol", "SYMBOL");
            if (header.Count == 0)
                return [];

            var map = NseCsvHelper.HeaderMap(header);
            var lines = NseCsvHelper.SplitLines(csv);
            var result = new List<NseMcapRow>(lines.Length);

            for (var i = start; i < lines.Length; i++)
            {
                var cols = NseCsvHelper.SplitCsv(lines[i]);
                var symbol = NseCsvHelper.Get(cols, map, "Symbol", "SYMBOL").ToUpperInvariant();
                if (string.IsNullOrWhiteSpace(symbol))
                    continue;

                var series = NseCsvHelper.Get(cols, map, "Series", "SERIES").ToUpperInvariant();
                if (!string.IsNullOrEmpty(series) && series != "EQ" && !MarketCapCategoryHelper.IsSmeSeries(series))
                    continue;

                var mcapRaw = NseCsvHelper.ParseDecimal(NseCsvHelper.Get(cols, map,
                    "Market Cap(Rs.)", "Market Cap (Rs.)", "Market Cap", "MarketCap", "MktCap"));

                if (mcapRaw <= 0)
                {
                    var issue = NseCsvHelper.ParseDecimal(NseCsvHelper.Get(cols, map, "Issue Size", "IssueSize"));
                    var close = NseCsvHelper.ParseDecimal(NseCsvHelper.Get(cols, map,
                        "Close Price/Paid up value(Rs.)", "Close Price", "Close"));
                    if (issue > 0 && close > 0)
                        mcapRaw = issue * close;
                }

                if (mcapRaw <= 0)
                    continue;

                result.Add(new NseMcapRow
                {
                    Symbol = symbol,
                    Series = string.IsNullOrEmpty(series) ? "EQ" : series,
                    MarketCapCrore = NseCsvHelper.ToCrore(mcapRaw)
                });
            }

            return result;
        }

        public static IReadOnlyList<NsePeRow> ParsePe(string csv)
        {
            var (header, start) = NseCsvHelper.FindHeader(csv, "Symbol", "SYMBOL");
            if (header.Count == 0)
                return [];

            var map = NseCsvHelper.HeaderMap(header);
            var lines = NseCsvHelper.SplitLines(csv);
            var result = new List<NsePeRow>(lines.Length);

            for (var i = start; i < lines.Length; i++)
            {
                var cols = NseCsvHelper.SplitCsv(lines[i]);
                var symbol = NseCsvHelper.Get(cols, map, "Symbol", "SYMBOL").ToUpperInvariant();
                if (string.IsNullOrWhiteSpace(symbol))
                    continue;

                var pe = NseCsvHelper.ParseDecimal(NseCsvHelper.Get(cols, map, "P/E", "PE", "PE_RATIO", "PERatio", "AdjustedPE"));
                var eps = NseCsvHelper.ParseDecimal(NseCsvHelper.Get(cols, map, "EPS", "Eps", "AdjustedEPS", "EarningPerShare"));
                if (pe <= 0 && eps <= 0)
                    continue;

                result.Add(new NsePeRow
                {
                    Symbol = symbol,
                    Series = NseCsvHelper.Get(cols, map, "Series", "SERIES").ToUpperInvariant(),
                    PE = pe,
                    EPS = eps
                });
            }

            return result;
        }

        public static IReadOnlyList<NseWeek52Row> ParseWeek52(string csv)
        {
            var (header, start) = NseCsvHelper.FindHeader(csv, "Symbol", "SYMBOL");
            if (header.Count == 0)
                return [];

            var map = NseCsvHelper.HeaderMap(header);
            var lines = NseCsvHelper.SplitLines(csv);
            var result = new List<NseWeek52Row>(lines.Length);

            for (var i = start; i < lines.Length; i++)
            {
                var cols = NseCsvHelper.SplitCsv(lines[i]);
                var symbol = NseCsvHelper.Get(cols, map, "Symbol", "SYMBOL").ToUpperInvariant();
                if (string.IsNullOrWhiteSpace(symbol))
                    continue;

                var high = NseCsvHelper.ParseDecimal(NseCsvHelper.Get(cols, map,
                    "Adjusted 52_Week_High", "Adjusted52_Week_High", "52_Week_High", "52WeekHigh", "YearHigh"));
                var low = NseCsvHelper.ParseDecimal(NseCsvHelper.Get(cols, map,
                    "Adjusted 52_Week_Low", "Adjusted52_Week_Low", "52_Week_Low", "52WeekLow", "YearLow"));
                if (high <= 0 && low <= 0)
                    continue;

                result.Add(new NseWeek52Row
                {
                    Symbol = symbol,
                    Series = NseCsvHelper.Get(cols, map, "Series", "SERIES").ToUpperInvariant(),
                    High = high,
                    Low = low
                });
            }

            return result;
        }

        public static IReadOnlyList<NseIndexMemberRow> ParseIndexMembers(string csv, string category)
        {
            var (header, start) = NseCsvHelper.FindHeader(csv, "Symbol", "SYMBOL");
            if (header.Count == 0)
                return [];

            var map = NseCsvHelper.HeaderMap(header);
            var lines = NseCsvHelper.SplitLines(csv);
            var result = new List<NseIndexMemberRow>(lines.Length);

            for (var i = start; i < lines.Length; i++)
            {
                var cols = NseCsvHelper.SplitCsv(lines[i]);
                var symbol = NseCsvHelper.Get(cols, map, "Symbol", "SYMBOL").ToUpperInvariant();
                if (string.IsNullOrWhiteSpace(symbol))
                    continue;

                result.Add(new NseIndexMemberRow
                {
                    Symbol = symbol,
                    Industry = NseCsvHelper.Get(cols, map, "Industry", "INDUSTRY"),
                    Category = category
                });
            }

            return result;
        }
    }
}
