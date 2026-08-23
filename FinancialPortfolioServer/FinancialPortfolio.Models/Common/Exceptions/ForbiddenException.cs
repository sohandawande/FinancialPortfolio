namespace FinancialPortfolio.Models.Common.Exceptions
{
    public sealed class ForbiddenException : BaseException
    {
        public ForbiddenException(string message)
            : base(message)
        {
        }
    }
}
