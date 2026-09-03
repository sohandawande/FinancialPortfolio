using FinancialPortfolio.Models.Common.Response;
using FinancialPortfolio.Models.Model.Request.Wealth;
using FinancialPortfolio.Models.Model.Response.Wealth;

namespace FinancialPortfolio.Business.Abstractions.IWealth
{
    public interface IWealthTrackerService
    {
        Task<ApiResponse<WealthSummaryResponse>> GetSummaryAsync(CancellationToken cancellationToken);

        Task<ApiResponse<List<MutualFundResponse>>> GetMutualFundsAsync(CancellationToken cancellationToken);
        Task<ApiResponse<MutualFundResponse>> AddMutualFundAsync(UpsertMutualFundRequest request, CancellationToken cancellationToken);
        Task<ApiResponse<MutualFundResponse>> UpdateMutualFundAsync(long id, UpsertMutualFundRequest request, CancellationToken cancellationToken);
        Task<ApiResponse<bool>> DeleteMutualFundAsync(long id, CancellationToken cancellationToken);

        Task<ApiResponse<List<FixedDepositResponse>>> GetFixedDepositsAsync(CancellationToken cancellationToken);
        Task<ApiResponse<FixedDepositResponse>> AddFixedDepositAsync(UpsertFixedDepositRequest request, CancellationToken cancellationToken);
        Task<ApiResponse<FixedDepositResponse>> UpdateFixedDepositAsync(long id, UpsertFixedDepositRequest request, CancellationToken cancellationToken);
        Task<ApiResponse<bool>> DeleteFixedDepositAsync(long id, CancellationToken cancellationToken);

        Task<ApiResponse<List<RecurringDepositResponse>>> GetRecurringDepositsAsync(CancellationToken cancellationToken);
        Task<ApiResponse<RecurringDepositResponse>> GetRecurringDepositDetailAsync(long id, CancellationToken cancellationToken);
        Task<ApiResponse<RecurringDepositResponse>> AddRecurringDepositAsync(UpsertRecurringDepositRequest request, CancellationToken cancellationToken);
        Task<ApiResponse<RecurringDepositResponse>> UpdateRecurringDepositAsync(long id, UpsertRecurringDepositRequest request, CancellationToken cancellationToken);
        Task<ApiResponse<bool>> DeleteRecurringDepositAsync(long id, CancellationToken cancellationToken);

        Task<ApiResponse<List<RdInstallmentResponse>>> GetRdInstallmentsAsync(long rdId, CancellationToken cancellationToken);
        Task<ApiResponse<RdInstallmentResponse>> PayRdInstallmentAsync(long rdId, PayRdInstallmentRequest request, CancellationToken cancellationToken);
        Task<ApiResponse<RdInstallmentResponse>> UpsertRdInstallmentAsync(long rdId, UpsertRdInstallmentRequest request, CancellationToken cancellationToken);
        Task<ApiResponse<bool>> DeleteRdInstallmentAsync(long rdId, long installmentId, CancellationToken cancellationToken);

        Task<ApiResponse<List<InsurancePolicyResponse>>> GetInsurancePoliciesAsync(CancellationToken cancellationToken);
        Task<ApiResponse<InsurancePolicyResponse>> AddInsurancePolicyAsync(UpsertInsurancePolicyRequest request, CancellationToken cancellationToken);
        Task<ApiResponse<InsurancePolicyResponse>> UpdateInsurancePolicyAsync(long id, UpsertInsurancePolicyRequest request, CancellationToken cancellationToken);
        Task<ApiResponse<bool>> DeleteInsurancePolicyAsync(long id, CancellationToken cancellationToken);
    }
}
