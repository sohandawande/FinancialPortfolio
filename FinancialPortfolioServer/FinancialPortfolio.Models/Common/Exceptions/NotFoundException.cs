namespace FinancialPortfolio.Models.Common.Exceptions
{
    public sealed class NotFoundException : BaseException
    {
        public NotFoundException(string message)
            : base(message)
        {
        }
    }
}
