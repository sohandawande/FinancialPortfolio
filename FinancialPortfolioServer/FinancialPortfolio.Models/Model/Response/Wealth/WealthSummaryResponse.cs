namespace FinancialPortfolio.Models.Model.Response.Wealth
{
    public class WealthBucketResponse
    {
        public string Key { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public decimal Invested { get; set; }
        public decimal CurrentValue { get; set; }
        public decimal GainLoss { get; set; }
        public decimal AllocationPercent { get; set; }
        public int Count { get; set; }
    }

    public class WealthSummaryResponse
    {
        public decimal TotalInvested { get; set; }
        public decimal TotalCurrentValue { get; set; }
        public decimal TotalGainLoss { get; set; }
        public decimal TotalGainLossPercent { get; set; }
        public List<WealthBucketResponse> Buckets { get; set; } = [];
        public List<MutualFundResponse> MutualFunds { get; set; } = [];
        public List<FixedDepositResponse> FixedDeposits { get; set; } = [];
        public List<RecurringDepositResponse> RecurringDeposits { get; set; } = [];
        public List<InsurancePolicyResponse> InsurancePolicies { get; set; } = [];
    }
}
