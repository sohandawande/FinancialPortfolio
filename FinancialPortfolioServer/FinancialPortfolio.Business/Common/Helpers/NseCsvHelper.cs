using System.Globalization;
using System.Text;

namespace FinancialPortfolio.Business.Common.Helpers
{
    /// <summary>
    /// Shared CSV helpers for NSE archive files (title rows, quoted fields, Indian number formats).
    /// </summary>
    public static class NseCsvHelper
    {
        public static string[] SplitLines(string csv)
            => csv.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        public static List<string> SplitCsv(string line)
        {
            var result = new List<string>();
            var current = new StringBuilder();
            var inQuotes = false;

            foreach (var ch in line)
            {
                if (ch == '"')
                {
                    inQuotes = !inQuotes;
                    continue;
                }

                if (ch == ',' && !inQuotes)
                {
                    result.Add(current.ToString());
                    current.Clear();
                    continue;
                }

                current.Append(ch);
            }

            result.Add(current.ToString());
            return result;
        }

        public static Dictionary<string, int> HeaderMap(IReadOnlyList<string> header)
        {
            var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < header.Count; i++)
            {
                var key = header[i].Trim();
                if (key.Length == 0)
                    continue;
                map[key] = i;
            }

            return map;
        }

        public static (List<string> Header, int DataStart) FindHeader(string csv, params string[] required)
        {
            var lines = SplitLines(csv);
            var max = Math.Min(lines.Length, 15);
            for (var i = 0; i < max; i++)
            {
                var cols = SplitCsv(lines[i]);
                if (cols.Any(c => required.Any(r => NamesMatch(c, r))))
                    return (cols, i + 1);
            }

            return ([], 0);
        }

        public static string Get(IReadOnlyList<string> cols, IReadOnlyDictionary<string, int> map, params string[] names)
        {
            foreach (var name in names)
            {
                if (map.TryGetValue(name, out var index) && index < cols.Count)
                    return cols[index].Trim();
            }

            foreach (var kv in map)
            {
                foreach (var name in names)
                {
                    if (NamesMatch(kv.Key, name) && kv.Value < cols.Count)
                        return cols[kv.Value].Trim();
                }
            }

            return string.Empty;
        }

        public static decimal ParseDecimal(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value is "-" or "NA" or "N/A" or "#N/A")
                return 0m;

            return decimal.TryParse(
                value.Replace(",", "", StringComparison.Ordinal),
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out var number)
                ? number
                : 0m;
        }

        public static long ParseLong(string value)
            => long.TryParse(
                value.Replace(",", "", StringComparison.Ordinal),
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out var number)
                ? number
                : 0L;

        public static DateOnly? ParseDate(string value)
        {
            if (DateOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                return date;
            if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTime))
                return DateOnly.FromDateTime(dateTime);
            return null;
        }

        /// <summary>NSE MCAP is rupees. Values already in crore stay as-is.</summary>
        public static decimal ToCrore(decimal value)
            => value >= 10_000_000m
                ? Math.Round(value / 10_000_000m, 5)
                : Math.Round(value, 5);

        public static HashSet<string> AllowedSeries(string[]? series)
            => new(
                (series ?? []).Select(s => s.Trim().ToUpperInvariant()).Where(s => s.Length > 0),
                StringComparer.OrdinalIgnoreCase);

        public static string Truncate(string value, int max)
            => value.Length <= max ? value : value[..max].Trim();

        private static bool NamesMatch(string left, string right)
        {
            static string Compact(string value)
                => value.Replace(" ", "", StringComparison.OrdinalIgnoreCase).Trim();

            return left.Equals(right, StringComparison.OrdinalIgnoreCase)
                   || Compact(left).Equals(Compact(right), StringComparison.OrdinalIgnoreCase);
        }
    }
}
