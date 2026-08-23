namespace FinancialPortfolio.Business.Abstractions.IAuthentication
{
    using FinancialPortfolio.Models.Common.Response;
    using FinancialPortfolio.Models.Model.Request.Authentication;
    using FinancialPortfolio.Models.Model.Response.Authentication;
    using System;

    public interface IAuthenticationService
    {
        Task<ApiResponse<LoginResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
        Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
        Task<ApiResponse<LoginResponse>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> LogoutAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> ChangePasswordAsync(Guid identityUserId, ChangePasswordRequest request, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> IsEmailAvailableAsync(string email, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> IsUserNameAvailableAsync(string userName, CancellationToken cancellationToken = default);
    }
}
