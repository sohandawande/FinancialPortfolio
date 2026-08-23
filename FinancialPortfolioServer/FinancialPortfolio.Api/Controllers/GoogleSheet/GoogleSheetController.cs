namespace FinancialPortfolio.Api.Controllers.GoogleSheet
{
    using FinancialPortfolio.Api.Common.Constants.Routes;
    using FinancialPortfolio.Api.Controllers.Base;
    using FinancialPortfolio.Business.Abstractions.IGoogleSheet;
    using FinancialPortfolio.Models.Abstractions.ICurrentUser;
    using FinancialPortfolio.Models.Common.Response;
    using FinancialPortfolio.Models.Common.Utilities;
    using FinancialPortfolio.Models.Model.Request.GoogleSheet;
    using FinancialPortfolio.Models.Model.Response.GoogleSheet;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Defines the <see cref="GoogleSheetController" />
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    public sealed class GoogleSheetController : BaseApiController
    {
        /// <summary>
        /// Defines the _googleSheetService
        /// </summary>
        private readonly IGoogleSheetService _googleSheetService;

        /// <summary>
        /// Initializes a new instance of the <see cref="GoogleSheetController"/> class.
        /// </summary>
        /// <param name="googleSheetService">The googleSheetService<see cref="IGoogleSheetService"/></param>
        /// <param name="currentUser">The currentUser<see cref="ICurrentUserService"/></param>
        public GoogleSheetController(IGoogleSheetService googleSheetService, ICurrentUserService currentUser) : base(currentUser)
        {
            _googleSheetService = Guard.AgainstNull(googleSheetService, nameof(googleSheetService));
        }

        /// <summary>
        /// Sync stocks from Google Sheet (Admin Only)
        /// </summary>
        /// <param name="request">The request<see cref="GoogleSheetRequest"/></param>
        /// <param name="cancellationToken">The cancellationToken<see cref="CancellationToken"/></param>
        /// <returns>The <see cref="Task{IActionResult}"/></returns>
        [HttpPost(GoogleSheetRoutes.Sync)]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<GoogleSheetSyncResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> SyncStocks([FromBody] GoogleSheetRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _googleSheetService.SyncStocksAsync(request, cancellationToken);
            return Ok(result);
        }
    }
}
