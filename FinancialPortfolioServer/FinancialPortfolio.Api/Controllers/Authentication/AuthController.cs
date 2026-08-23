namespace FinancialPortfolio.Api.Controllers.Auth
{
    using FinancialPortfolio.Api.Common.Constants.Routes;
    using FinancialPortfolio.Api.Controllers.Base;
    using FinancialPortfolio.Business.Abstractions.IAuthentication;
    using FinancialPortfolio.Models.Abstractions.ICurrentUser;
    using FinancialPortfolio.Models.Common.Response;
    using FinancialPortfolio.Models.Common.Utilities;
    using FinancialPortfolio.Models.Model.Request.Authentication;
    using FinancialPortfolio.Models.Model.Response.Authentication;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Defines the <see cref="AuthController" />
    /// </summary>
    [Route("api/[controller]")]
    public sealed class AuthController : BaseApiController
    {
        /// <summary>
        /// Defines the _authenticationService
        /// </summary>
        private readonly IAuthenticationService _authenticationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthController"/> class.
        /// </summary>
        /// <param name="authenticationService">The authenticationService<see cref="IAuthenticationService"/></param>
        /// <param name="currentUser">The currentUser<see cref="ICurrentUserService"/></param>
        public AuthController(IAuthenticationService authenticationService, ICurrentUserService currentUser) : base(currentUser)
        {
            _authenticationService = Guard.AgainstNull(authenticationService, nameof(authenticationService));
        }

        /// <summary>
        /// The Register
        /// </summary>
        /// <param name="request">The request<see cref="RegisterRequest"/></param>
        /// <param name="cancellationToken">The cancellationToken<see cref="CancellationToken"/></param>
        /// <returns>The <see cref="Task{IActionResult}"/></returns>
        [HttpPost(AuthRoutes.Register)]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
        {
            var result = await _authenticationService.RegisterAsync(request, cancellationToken);

            return Success(result);
        }

        /// <summary>
        /// The Login
        /// </summary>
        /// <param name="request">The request<see cref="LoginRequest"/></param>
        /// <param name="cancellationToken">The cancellationToken<see cref="CancellationToken"/></param>
        /// <returns>The <see cref="Task{IActionResult}"/></returns>
        [HttpPost(AuthRoutes.Login)]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
        {
            var result = await _authenticationService.LoginAsync(request, cancellationToken);

            return Success(result);
        }

        /// <summary>
        /// The RefreshToken
        /// </summary>
        /// <param name="request">The request<see cref="RefreshTokenRequest"/></param>
        /// <param name="cancellationToken">The cancellationToken<see cref="CancellationToken"/></param>
        /// <returns>The <see cref="Task{IActionResult}"/></returns>
        [HttpPost(AuthRoutes.RefreshToken)]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
        {
            var result = await _authenticationService.RefreshTokenAsync(request, cancellationToken);

            return Success(result);
        }

        /// <summary>
        /// The Logout
        /// </summary>
        /// <param name="cancellationToken">The cancellationToken<see cref="CancellationToken"/></param>
        /// <returns>The <see cref="Task{IActionResult}"/></returns>
        [HttpPost(AuthRoutes.Logout)]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Logout(CancellationToken cancellationToken)
        {
            var result = await _authenticationService.LogoutAsync(CurrentUser.IdentityUserId, cancellationToken);

            return Success(result);
        }

        /// <summary>
        /// The Me
        /// </summary>
        /// <returns>The <see cref="IActionResult"/></returns>
        [HttpGet(AuthRoutes.Me)]
        [Authorize]
        [ProducesResponseType(typeof(CurrentUserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult Me()
        {
            var response = new CurrentUserResponse
            {
                IdentityUserId = CurrentUser.IdentityUserId,
                UserId = CurrentUser.UserId,
                FullName = CurrentUser.FullName,
                UserCode = CurrentUser.UserCode,
                Email = CurrentUser.Email,
                UserName = CurrentUser.UserName,
                Roles = CurrentUser.Roles
            };

            return Ok(response);
        }

        /// <summary>
        /// Request password reset email
        /// </summary>
        [HttpPost(AuthRoutes.ForgotPassword)]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken)
        {
            var result = await _authenticationService.ForgotPasswordAsync(request, cancellationToken);
            return Success(result);
        }

        /// <summary>
        /// Reset password with token from email
        /// </summary>
        [HttpPost(AuthRoutes.ResetPassword)]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
        {
            var result = await _authenticationService.ResetPasswordAsync(request, cancellationToken);
            return Success(result);
        }

        /// <summary>
        /// Change password for logged-in user
        /// </summary>
        [HttpPost(AuthRoutes.ChangePassword)]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
        {
            var result = await _authenticationService.ChangePasswordAsync(CurrentUser.IdentityUserId, request, cancellationToken);
            return Success(result);
        }

        /// <summary>
        /// The CheckEmail
        /// </summary>
        /// <param name="email">The email<see cref="string"/></param>
        /// <param name="cancellationToken">The cancellationToken<see cref="CancellationToken"/></param>
        /// <returns>The <see cref="Task{IActionResult}"/></returns>
        [HttpGet(AuthRoutes.CheckEmail)]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CheckEmail([FromQuery] string email, CancellationToken cancellationToken)
        {
            var result = await _authenticationService.IsEmailAvailableAsync(email, cancellationToken);
            return Success(result);
        }

        /// <summary>
        /// The CheckUserName
        /// </summary>
        /// <param name="userName">The userName<see cref="string"/></param>
        /// <param name="cancellationToken">The cancellationToken<see cref="CancellationToken"/></param>
        /// <returns>The <see cref="Task{IActionResult}"/></returns>
        [HttpGet(AuthRoutes.CheckUserName)]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CheckUserName([FromQuery] string userName, CancellationToken cancellationToken)
        {
            var result = await _authenticationService.IsUserNameAvailableAsync(userName, cancellationToken);
            return Success(result);
        }
    }
}
