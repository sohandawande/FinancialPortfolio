namespace FinancialPortfolio.Models.Common.Exceptions
{
    public sealed class UnauthorizedException : BaseException
    {
        public UnauthorizedException(string message)
            : base(message)
        {
        }
    }
}
