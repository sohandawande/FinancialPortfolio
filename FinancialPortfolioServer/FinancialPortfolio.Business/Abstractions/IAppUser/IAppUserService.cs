using FinancialPortfolio.Models.Common.Response;
using FinancialPortfolio.Models.Model.Request.User;
using FinancialPortfolio.Models.Model.Response.AppUser;
using FinancialPortfolio.Models.Model.Response.User;

namespace FinancialPortfolio.Business.Abstractions.IAppUser
{
    public interface IAppUserService
    {
        Task<ApiResponse<List<PendingUserResponse>>> GetPendingUsersAsync(CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> AssignRoleAsync(Guid identityUserId, AssignRoleRequest request, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> ApproveUserAsync(Guid identityUserId, AssignRoleRequest request, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> ActivateUserAsync(Guid identityUserId, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> DeactivateUserAsync(Guid identityUserId, CancellationToken cancellationToken = default);
        Task<ApiResponse<List<ManagedUserResponse>>> GetManagedUsersAsync(CancellationToken cancellationToken = default);
        Task<ApiResponse<UserDetailResponse>> GetUserByIdAsync(Guid identityUserId, CancellationToken cancellationToken = default);
    }
}
