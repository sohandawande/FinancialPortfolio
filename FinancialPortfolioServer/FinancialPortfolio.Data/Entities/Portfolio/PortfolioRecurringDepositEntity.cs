using FinancialPortfolio.Data.Common.Base;
using FinancialPortfolio.Models.Common.Enums;

namespace FinancialPortfolio.Data.Entities.Portfolio
{
    public sealed class PortfolioRecurringDepositEntity : BaseEntity
    {
        public long Id { get; set; }
        public long PortfolioId { get; set; }
        public string BankName { get; set; } = string.Empty;
        public string? BankIfsc { get; set; }
        public string? AccountRef { get; set; }
        public string? LinkedAccountNumber { get; set; }
        public string? LinkedIfsc { get; set; }
        public decimal MonthlyAmount { get; set; }
        public decimal InterestRate { get; set; }
        public int TenureMonths { get; set; }
        public int InstallmentsPaid { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime MaturityDate { get; set; }
        public DepositStatus Status { get; set; } = DepositStatus.Active;
        public string? Notes { get; set; }

        public PortfolioEntity Portfolio { get; set; } = default!;
        public ICollection<PortfolioRecurringDepositInstallmentEntity> Installments { get; set; } = [];
    }
}
