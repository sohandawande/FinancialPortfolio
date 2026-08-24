namespace FinancialPortfolio.Api.Controllers.MarketData
{
    using FinancialPortfolio.Api.Common.Constants.Routes;
    using FinancialPortfolio.Api.Controllers.Base;
    using FinancialPortfolio.Business.Abstractions.IMarketData;
    using FinancialPortfolio.Models.Abstractions.ICurrentUser;
    using FinancialPortfolio.Models.Common.Response;
    using FinancialPortfolio.Models.Common.Utilities;
    using FinancialPortfolio.Models.Model.Request.MarketData;
    using FinancialPortfolio.Models.Model.Response.MarketData;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [Authorize]
    [Route("api/[controller]")]
    public sealed class MarketDataController : BaseApiController
    {
        private readonly INseBhavcopyService _nseBhavcopyService;

        public MarketDataController(INseBhavcopyService nseBhavcopyService, ICurrentUserService currentUser)
            : base(currentUser)
        {
            _nseBhavcopyService = Guard.AgainstNull(nseBhavcopyService, nameof(nseBhavcopyService));
        }

        /// <summary>
        /// Sync equity delivery EOD prices from official NSE bhavcopy.
        /// </summary>
        [HttpPost(MarketDataRoutes.Sync)]
        [ProducesResponseType(typeof(ApiResponse<MarketDataSyncResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Sync([FromBody] MarketDataSyncRequest? request, CancellationToken cancellationToken)
        {
            var result = await _nseBhavcopyService.SyncAsync(request ?? new MarketDataSyncRequest(), cancellationToken);
            return Success(result);
        }
    }
}
