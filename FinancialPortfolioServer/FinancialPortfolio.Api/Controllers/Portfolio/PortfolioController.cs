using FinancialPortfolio.Api.Controllers.Base;
using FinancialPortfolio.Business.Abstractions.IPortfolio;
using FinancialPortfolio.Models.Abstractions.ICurrentUser;
using FinancialPortfolio.Models.Common.Utilities;
using FinancialPortfolio.Models.Model.Request.Portfolio;
using Microsoft.AspNetCore.Mvc;

namespace FinancialPortfolio.Api.Controllers.Portfolio
{
    [Route("api/[controller]")]
    public class PortfolioController : BaseApiController
    {
        private readonly IPortfolioService _portfolioService;

        public PortfolioController(IPortfolioService portfolioService, ICurrentUserService currentUserService)
            : base(currentUserService)
        {
            _portfolioService = Guard.AgainstNull(portfolioService, nameof(portfolioService));
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            var result = await _portfolioService.GetAsync(cancellationToken);
            return Ok(result);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreatePortfolioRequest request, CancellationToken cancellationToken)
        {
            var result = await _portfolioService.CreateAsync(request, cancellationToken);
            return Ok(result);
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update([FromBody] UpdatePortfolioRequest request, CancellationToken cancellationToken)
        {
            var result = await _portfolioService.UpdateAsync(request, cancellationToken);
            return Ok(result);
        }

        [HttpGet("summary")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSummary(CancellationToken cancellationToken)
        {
            var result = await _portfolioService.GetSummaryAsync(cancellationToken);
            return Ok(result);
        }

        [HttpGet("holdings")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetHoldings(CancellationToken cancellationToken)
        {
            var result = await _portfolioService.GetHoldingsAsync(cancellationToken);
            return Ok(result);
        }

        [HttpGet("sold")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSoldHistory(CancellationToken cancellationToken)
        {
            var result = await _portfolioService.GetSoldHistoryAsync(cancellationToken);
            return Ok(result);
        }

        [HttpPost("buy")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Buy([FromBody] BuyStockRequest request, CancellationToken cancellationToken)
        {
            var result = await _portfolioService.BuyAsync(request, cancellationToken);
            return Ok(result);
        }

        [HttpPut("hold/{id:long}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateHold(long id, [FromBody] UpdateHoldRequest request, CancellationToken cancellationToken)
        {
            var result = await _portfolioService.UpdateHoldAsync(id, request, cancellationToken);
            return Ok(result);
        }

        [HttpDelete("hold/{id:long}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteHold(long id, CancellationToken cancellationToken)
        {
            var result = await _portfolioService.DeleteHoldAsync(id, cancellationToken);
            return Ok(result);
        }

        [HttpPost("sell")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Sell([FromBody] SellStockRequest request, CancellationToken cancellationToken)
        {
            var result = await _portfolioService.SellAsync(request, cancellationToken);
            return Ok(result);
        }

        [HttpPut("sold/{id:long}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateSold(long id, [FromBody] UpdateSoldRequest request, CancellationToken cancellationToken)
        {
            var result = await _portfolioService.UpdateSoldAsync(id, request, cancellationToken);
            return Ok(result);
        }

        [HttpDelete("sold/{id:long}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteSold(long id, CancellationToken cancellationToken)
        {
            var result = await _portfolioService.DeleteSoldAsync(id, cancellationToken);
            return Ok(result);
        }

        [HttpGet("ledger")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetLedger([FromQuery] string? type, CancellationToken cancellationToken)
        {
            var result = await _portfolioService.GetLedgerAsync(type, cancellationToken);
            return Ok(result);
        }

        [HttpGet("positions")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPositions([FromQuery] string? status, CancellationToken cancellationToken)
        {
            var result = await _portfolioService.GetPositionsAsync(status, cancellationToken);
            return Ok(result);
        }

        [HttpGet("positions/{stockId:long}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPositionDetail(long stockId, CancellationToken cancellationToken)
        {
            var result = await _portfolioService.GetPositionDetailAsync(stockId, cancellationToken);
            return Ok(result);
        }

        [HttpGet("dividends")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDividends([FromQuery] long? stockId, CancellationToken cancellationToken)
        {
            var result = await _portfolioService.GetDividendsAsync(stockId, cancellationToken);
            return Ok(result);
        }

        [HttpGet("dividends/overview")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDividendOverview(CancellationToken cancellationToken)
        {
            var result = await _portfolioService.GetDividendOverviewAsync(cancellationToken);
            return Ok(result);
        }

        [HttpPost("dividend")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddDividend([FromBody] AddDividendRequest request, CancellationToken cancellationToken)
        {
            var result = await _portfolioService.AddDividendAsync(request, cancellationToken);
            return Ok(result);
        }

        [HttpPut("dividend/{id:long}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateDividend(long id, [FromBody] UpdateDividendRequest request, CancellationToken cancellationToken)
        {
            var result = await _portfolioService.UpdateDividendAsync(id, request, cancellationToken);
            return Ok(result);
        }

        [HttpDelete("dividend/{id:long}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteDividend(long id, CancellationToken cancellationToken)
        {
            var result = await _portfolioService.DeleteDividendAsync(id, cancellationToken);
            return Ok(result);
        }
    }
}
