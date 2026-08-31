using FinancialPortfolio.Models.Common.Response;
using FinancialPortfolio.Models.Model.Response.Wealth;

namespace FinancialPortfolio.Business.Abstractions.IWealth
{
    public interface IBankLookupService
    {
        Task<ApiResponse<BankIfscResponse?>> LookupIfscAsync(string ifsc, CancellationToken cancellationToken);
        Task<ApiResponse<List<BankSuggestionResponse>>> SearchBanksAsync(string query, CancellationToken cancellationToken);
    }
}
