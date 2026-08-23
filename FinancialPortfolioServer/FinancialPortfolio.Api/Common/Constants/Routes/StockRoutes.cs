namespace FinancialPortfolio.Api.Common.Constants.Routes
{
    public static class StockRoutes
    {
        public const string Search = "search";
        public const string GetById = "get-by-id/{id:long}";
        public const string Create = "create";
        public const string Update = "update/{id:long}";
        public const string Delete = "delete/{id:long}";
        public const string Logo = "logo/{symbol}";
    }
}
