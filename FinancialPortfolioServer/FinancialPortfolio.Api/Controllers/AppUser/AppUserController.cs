namespace FinancialPortfolio.Api.Controllers.User
{
    using FinancialPortfolio.Api.Common.Constants.Routes;
    using FinancialPortfolio.Api.Controllers.Base;
    using FinancialPortfolio.Business.Abstractions.IAppUser;
    using FinancialPortfolio.Models.Abstractions.ICurrentUser;
    using FinancialPortfolio.Models.Common.Response;
    using FinancialPortfolio.Models.Common.Utilities;
    using FinancialPortfolio.Models.Model.Request.User;
    using FinancialPortfolio.Models.Model.Response.AppUser;
    using FinancialPortfolio.Models.Model.Response.User;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Defines the <see cref="AppUserController" />
    /// </summary>
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    public class AppUserController : BaseApiController
    {
        /// <summary>
        /// Defines the _userService
        /// </summary>
        private readonly IAppUserService _userService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppUserController"/> class.
        /// </summary>
        /// <param name="userService">The userService<see cref="IAppUserService"/></param>
        /// <param name="currentUserService">The currentUserService<see cref="ICurrentUserService"/></param>
        public AppUserController(IAppUserService userService, ICurrentUserService currentUserService) : base(currentUserService)
        {
            _userService = Guard.AgainstNull(userService, nameof(userService));
        }

        /// <summary>
        /// Get all users waiting for approval
        /// </summary>
        /// <param name="cancellationToken">The cancellationToken<see cref="CancellationToken"/></param>
        /// <returns>The <see cref="Task{IActionResult}"/></returns>
        [HttpGet(AppUserRoutes.Pending)]
        [ProducesResponseType(typeof(ApiResponse<List<PendingUserResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPendingUsers(CancellationToken cancellationToken)
        {
            var response = await _userService.GetPendingUsersAsync(cancellationToken);
            return Success(response);
        }

        /// <summary>
        /// Approve a pending user and assign roles
        /// </summary>
        /// <param name="identityUserId">The identityUserId<see cref="Guid"/></param>
        /// <param name="request">The request<see cref="AssignRoleRequest"/></param>
        /// <param name="cancellationToken">The cancellationToken<see cref="CancellationToken"/></param>
        /// <returns>The <see cref="Task{IActionResult}"/></returns>
        [HttpPut(AppUserRoutes.Approve)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> ApproveUser(Guid identityUserId, [FromBody] AssignRoleRequest request, CancellationToken cancellationToken)
        {
            var response = await _userService.ApproveUserAsync(identityUserId, request, cancellationToken);

            return Ok(response);
        }

        /// <summary>
        /// Change roles of an existing user
        /// </summary>
        /// <param name="identityUserId">The identityUserId<see cref="Guid"/></param>
        /// <param name="request">The request<see cref="AssignRoleRequest"/></param>
        /// <param name="cancellationToken">The cancellationToken<see cref="CancellationToken"/></param>
        /// <returns>The <see cref="Task{IActionResult}"/></returns>
        [HttpPut(AppUserRoutes.AssignRoles)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> AssignRole(Guid identityUserId, [FromBody] AssignRoleRequest request, CancellationToken cancellationToken)
        {
            var response = await _userService.AssignRoleAsync(identityUserId, request, cancellationToken);

            return Ok(response);
        }

        /// <summary>
        /// Activate a user
        /// </summary>
        /// <param name="identityUserId">The identityUserId<see cref="Guid"/></param>
        /// <param name="cancellationToken">The cancellationToken<see cref="CancellationToken"/></param>
        /// <returns>The <see cref="Task{IActionResult}"/></returns>
        [HttpPut(AppUserRoutes.Activate)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> ActivateUser(Guid identityUserId, CancellationToken cancellationToken)
        {
            var response = await _userService.ActivateUserAsync(identityUserId, cancellationToken);

            return Ok(response);
        }

        /// <summary>
        /// Deactivate a user
        /// </summary>
        /// <param name="identityUserId">The identityUserId<see cref="Guid"/></param>
        /// <param name="cancellationToken">The cancellationToken<see cref="CancellationToken"/></param>
        /// <returns>The <see cref="Task{IActionResult}"/></returns>
        [HttpPut(AppUserRoutes.Deactivate)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> DeactivateUser(Guid identityUserId, CancellationToken cancellationToken)
        {
            var response = await _userService.DeactivateUserAsync(identityUserId, cancellationToken);

            return Ok(response);
        }

        /// <summary>
        /// Get managed users (excluding pending-only registrations)
        /// </summary>
        [HttpGet(AppUserRoutes.ManageUsers)]
        [ProducesResponseType(typeof(ApiResponse<List<ManagedUserResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetManagedUsers(CancellationToken cancellationToken)
        {
            var response = await _userService.GetManagedUsersAsync(cancellationToken);
            return Success(response);
        }

        [HttpGet(AppUserRoutes.GetById)]
        [ProducesResponseType(typeof(ApiResponse<UserDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserById(Guid identityUserId, CancellationToken cancellationToken)
        {
            var response = await _userService.GetUserByIdAsync(identityUserId, cancellationToken);
            return Success(response);
        }
    }
}
