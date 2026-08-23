namespace FinancialPortfolio.Business.Common.Constants
{
    public static class GoogleSheetConstants
    {
        public const string NotAvailable = "#N/A";
        public const string Value = "#VALUE!";
        public const string Reference = "#REF!";
        public const string DivideByZero = "#DIV/0!";
        public const string Name = "#NAME?";
        public const string Number = "#NUM!";
        public const string Error = "#ERROR!";
        public const string Loading = "Loading...";

        public static readonly HashSet<string> ErrorValues =
        [
        NotAvailable,
        Value,
        Reference,
        DivideByZero,
        Name,
        Number,
        Error,
        Loading
        ];
    }
}
