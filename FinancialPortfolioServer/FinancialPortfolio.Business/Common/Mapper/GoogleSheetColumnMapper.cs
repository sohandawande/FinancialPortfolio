namespace FinancialPortfolio.Business.Common.Mapper
{
    public static class GoogleSheetColumnMapper
    {
        public static Dictionary<string, int> Build(IReadOnlyList<string> headers)
        {
            return headers
                .Select((value, index) => new { value, index })
                .ToDictionary(
                    x => x.value.Trim(),
                    x => x.index,
                    StringComparer.OrdinalIgnoreCase);
        }
    }
}
