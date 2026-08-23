namespace FinancialPortfolio.Api.Controllers.SystemLog
{
    using FinancialPortfolio.Api.Common.Constants.Routes;
    using FinancialPortfolio.Api.Controllers.Base;
    using FinancialPortfolio.Business.Abstractions.ILogger;
    using FinancialPortfolio.Models.Abstractions.ICurrentUser;
    using FinancialPortfolio.Models.Common.Exceptions;
    using FinancialPortfolio.Models.Common.Response;
    using FinancialPortfolio.Models.Common.Utilities;
    using FinancialPortfolio.Models.Model.Request.SystemLog;
    using FinancialPortfolio.Models.Model.Response.SystemLog;
    using FinancialPortfolio.QueryEngine.Models;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Defines the <see cref="SystemLogController" />
    /// </summary>
    [Route("api/[controller]")]
    [Authorize]
    public class SystemLogController : BaseApiController
    {
        /// <summary>
        /// Defines the _systemLogService
        /// </summary>
        private readonly ISystemLogService _systemLogService;

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemLogController"/> class.
        /// </summary>
        /// <param name="systemLogService">The systemLogService<see cref="ISystemLogService"/></param>
        /// <param name="currentUserService">The currentUserService<see cref="ICurrentUserService"/></param>
        public SystemLogController(ISystemLogService systemLogService, ICurrentUserService currentUserService) : base(currentUserService)
        {
            _systemLogService = Guard.AgainstNull(systemLogService, nameof(systemLogService));
        }

        /// <summary>
        /// Gets all system logs
        /// </summary>
        /// <param name="request">The request<see cref="QueryRequest"/></param>
        /// <param name="cancellationToken">The cancellationToken<see cref="CancellationToken"/></param>
        /// <returns>The <see cref="Task{IActionResult}"/></returns>
        [HttpPost(SystemLogRoutes.GetAll)]
        [ProducesResponseType(typeof(ApiResponse<List<SystemLogResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAll([FromBody] QueryRequest request, CancellationToken cancellationToken)
        {
            var result = await _systemLogService.GetAllAsync(request, cancellationToken);

            return Success(result);
        }

        /// <summary>
        /// Gets a system log by identifier
        /// </summary>
        /// <param name="id">The id<see cref="long"/></param>
        /// <param name="cancellationToken">The cancellationToken<see cref="CancellationToken"/></param>
        /// <returns>The <see cref="Task{IActionResult}"/></returns>
        [HttpGet(SystemLogRoutes.GetById)]
        [ProducesResponseType(typeof(ApiResponse<SystemLogResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(long id, CancellationToken cancellationToken)
        {
            var result = await _systemLogService.GetByIdAsync(id, cancellationToken);

            if (result is null)
            {
                throw new NotFoundException("Log not found.");
            }

            return Success(result);
        }

        /// <summary>
        /// Receives logs from Angular 22 client
        /// </summary>
        /// <param name="request">The request<see cref="ClientLogRequest"/></param>
        /// <param name="cancellationToken">The cancellationToken<see cref="CancellationToken"/></param>
        /// <returns>The <see cref="Task{IActionResult}"/></returns>
        [HttpPost(SystemLogRoutes.Client)]
        [AllowAnonymous]   // Change to [Authorize] if you only want authenticated users
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateClientLog([FromBody] ClientLogRequest request, CancellationToken cancellationToken)
        {
            var result = await _systemLogService.CreateClientLogAsync(request, cancellationToken);
            return Success(result);
        }
    }
}
