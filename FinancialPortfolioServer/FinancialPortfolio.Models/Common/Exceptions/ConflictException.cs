namespace FinancialPortfolio.Models.Common.Exceptions
{
    public sealed class ConflictException : BaseException
    {
        public ConflictException(string message)
            : base(message)
        {
        }
    }
}
