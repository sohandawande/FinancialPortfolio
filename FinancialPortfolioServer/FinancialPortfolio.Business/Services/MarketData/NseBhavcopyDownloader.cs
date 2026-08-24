using System.IO.Compression;
using System.Text;
using FinancialPortfolio.Business.Abstractions.IMarketData;
using FinancialPortfolio.Business.Common.Helpers;
using FinancialPortfolio.Models.Common.Utilities;
using FinancialPortfolio.Models.Model.Settings;
using Microsoft.Extensions.Options;

namespace FinancialPortfolio.Business.Services.MarketData
{
    /// <summary>
    /// Downloads official NSE market-data archives.
    /// Fundamental metrics are intentionally handled by a separate provider.
    /// </summary>
    public sealed class NseBhavcopyDownloader : INseBhavcopyDownloader
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly NseBhavcopySettings _settings;

        public NseBhavcopyDownloader(
            IHttpClientFactory httpClientFactory,
            IOptions<NseBhavcopySettings> options)
        {
            _httpClientFactory = Guard.AgainstNull(httpClientFactory, nameof(httpClientFactory));
            _settings = Guard.AgainstNull(options.Value, nameof(options));
        }

        public async Task<(DateOnly TradeDate, IReadOnlyList<NseBhavcopyRow> Rows)?> DownloadAsync(
            DateOnly? preferredDate,
            CancellationToken cancellationToken)
        {
            var start = preferredDate ?? DateOnly.FromDateTime(GetIstNow().Date);
            var lookback = Math.Max(1, _settings.LookbackDays);

            for (var i = 0; i < lookback; i++)
            {
                var date = start.AddDays(-i);
                if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                    continue;

                var nse = await TryDownloadNseAsync(date, cancellationToken) ?? [];
                var sme = await TryDownloadSmeAsync(date, cancellationToken);
                if (sme.Count > 0)
                    nse = MergeBySymbol(nse, sme);

                if (nse.Count > 0)
                    return (date, nse);
            }

            return null;
        }

        public Task<IReadOnlyList<NseWeek52Row>> DownloadWeek52Async(
            DateOnly tradeDate,
            CancellationToken cancellationToken)
            => DownloadFirstAsync(
                new[]
                {
                    $"https://nsearchives.nseindia.com/content/CM_52_wk_High_low_{tradeDate:ddMMyyyy}.csv",
                    $"https://archives.nseindia.com/content/CM_52_wk_High_low_{tradeDate:ddMMyyyy}.csv",
                },
                NseMarketFileParser.ParseWeek52,
                cancellationToken);

        public async Task<IReadOnlyList<NseIndexMemberRow>> DownloadIndexMembershipAsync(
            CancellationToken cancellationToken)
        {
            var files = new (string File, string Category)[]
            {
                ("ind_nifty100list.csv", MarketCapCategoryHelper.LargeCap),
                ("ind_niftymidcap150list.csv", MarketCapCategoryHelper.MidCap),
                ("ind_niftysmallcap250list.csv", MarketCapCategoryHelper.SmallCap),
                ("ind_niftymicrocap250list.csv", MarketCapCategoryHelper.MicroCap),
                ("ind_nifty500list.csv", string.Empty),
            };

            var map = new Dictionary<string, NseIndexMemberRow>(StringComparer.OrdinalIgnoreCase);

            foreach (var (file, category) in files)
            {
                var rows = await DownloadFirstAsync(
                    new[]
                    {
                        $"https://nsearchives.nseindia.com/content/indices/{file}",
                        $"https://archives.nseindia.com/content/indices/{file}",
                        $"https://www.niftyindices.com/IndexConstituent/{file}",
                    },
                    csv => NseMarketFileParser.ParseIndexMembers(csv, category),
                    cancellationToken);

                foreach (var row in rows)
                {
                    if (!map.TryGetValue(row.Symbol, out var existing))
                    {
                        map[row.Symbol] = row;
                        continue;
                    }

                    map[row.Symbol] = new NseIndexMemberRow
                    {
                        Symbol = row.Symbol,
                        Industry = string.IsNullOrWhiteSpace(existing.Industry) ? row.Industry : existing.Industry,
                        Category = string.IsNullOrWhiteSpace(existing.Category) ? row.Category : existing.Category
                    };
                }
            }

            return map.Values.ToList();
        }

        private async Task<IReadOnlyList<NseBhavcopyRow>?> TryDownloadNseAsync(
            DateOnly date,
            CancellationToken cancellationToken)
        {
            var urls = new[]
            {
                $"https://nsearchives.nseindia.com/content/cm/BhavCopy_NSE_CM_0_0_0_{date:yyyyMMdd}_F_0000.csv.zip",
                $"https://archives.nseindia.com/content/cm/BhavCopy_NSE_CM_0_0_0_{date:yyyyMMdd}_F_0000.csv.zip",
            };

            foreach (var url in urls)
            {
                var bytes = await TryGetBytesAsync(url, cancellationToken);
                var csv = ReadZipCsv(bytes);
                if (string.IsNullOrWhiteSpace(csv))
                    continue;

                var rows = NseMarketFileParser.ParseUdiff(csv, date, "NSE", _settings.NseSeries);
                if (rows.Count > 0)
                    return rows;
            }

            return null;
        }

        private Task<IReadOnlyList<NseBhavcopyRow>> TryDownloadSmeAsync(
            DateOnly date,
            CancellationToken cancellationToken)
            => DownloadFirstAsync(
                new[]
                {
                    $"https://nsearchives.nseindia.com/archives/sme/bhavcopy/sme{date:ddMMyy}.csv",
                    $"https://archives.nseindia.com/archives/sme/bhavcopy/sme{date:ddMMyy}.csv",
                },
                csv => NseMarketFileParser.ParseClassicBhav(csv, date, "NSE"),
                cancellationToken);

        private async Task<IReadOnlyList<T>> DownloadFirstAsync<T>(
            IEnumerable<string> urls,
            Func<string, IReadOnlyList<T>> parse,
            CancellationToken cancellationToken)
        {
            foreach (var url in urls)
            {
                var csv = await TryGetTextAsync(url, cancellationToken);
                if (string.IsNullOrWhiteSpace(csv))
                    continue;

                var rows = parse(csv);
                if (rows.Count > 0)
                    return rows;
            }

            return [];
        }

        private static IReadOnlyList<NseBhavcopyRow> MergeBySymbol(
            IReadOnlyList<NseBhavcopyRow> primary,
            IReadOnlyList<NseBhavcopyRow> extra)
        {
            var map = primary.ToDictionary(x => x.Symbol, StringComparer.OrdinalIgnoreCase);
            foreach (var row in extra)
            {
                if (!map.ContainsKey(row.Symbol))
                    map[row.Symbol] = row;
            }

            return map.Values.ToList();
        }

        private HttpClient CreateClient() => _httpClientFactory.CreateClient("NseArchives");

        private async Task<string?> TryGetTextAsync(
            string url,
            CancellationToken cancellationToken)
        {
            try
            {
                using var response = await CreateClient().GetAsync(url, cancellationToken);
                if (!response.IsSuccessStatusCode)
                    return null;

                var text = await response.Content.ReadAsStringAsync(cancellationToken);
                return string.IsNullOrWhiteSpace(text) || text.TrimStart().StartsWith('<') ? null : text;
            }
            catch
            {
                return null;
            }
        }

        private async Task<byte[]?> TryGetBytesAsync(
            string url,
            CancellationToken cancellationToken)
        {
            try
            {
                using var response = await CreateClient().GetAsync(url, cancellationToken);
                return response.IsSuccessStatusCode
                    ? await response.Content.ReadAsByteArrayAsync(cancellationToken)
                    : null;
            }
            catch
            {
                return null;
            }
        }

        private static string? ReadZipCsv(byte[]? bytes)
        {
            if (bytes is null || bytes.Length < 4 || bytes[0] != (byte)'P' || bytes[1] != (byte)'K')
                return null;

            using var zip = new ZipArchive(new MemoryStream(bytes), ZipArchiveMode.Read);
            var entry = zip.Entries.FirstOrDefault(e =>
                e.Name.EndsWith(".csv", StringComparison.OrdinalIgnoreCase));

            if (entry is null)
                return null;

            using var stream = entry.Open();
            using var reader = new StreamReader(stream, Encoding.UTF8);
            return reader.ReadToEnd();
        }

        private static DateTime GetIstNow()
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
        }
    }
}
