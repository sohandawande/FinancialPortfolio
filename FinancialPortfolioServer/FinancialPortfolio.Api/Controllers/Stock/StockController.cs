namespace FinancialPortfolio.Api.Controllers.Stock
{
    using FinancialPortfolio.Api.Common.Constants.Routes;
    using FinancialPortfolio.Api.Controllers.Base;
    using FinancialPortfolio.Business.Abstractions.IStock;
    using FinancialPortfolio.Models.Abstractions.ICurrentUser;
    using FinancialPortfolio.Models.Common.Exceptions;
    using FinancialPortfolio.Models.Common.Response;
    using FinancialPortfolio.Models.Common.Utilities;
    using FinancialPortfolio.Models.Model.Request.Stock;
    using FinancialPortfolio.Models.Model.Response.Stock;
    using FinancialPortfolio.QueryEngine.Models;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Defines the <see cref="StockController" />
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    public sealed class StockController : BaseApiController
    {
        /// <summary>
        /// Defines the _stockService
        /// </summary>
        private readonly IStockService _stockService;
        private readonly IWebHostEnvironment _env;

        /// <summary>
        /// Initializes a new instance of the <see cref="StockController"/> class.
        /// </summary>
        /// <param name="stockService">The stockService<see cref="IStockService"/></param>
        /// <param name="currentUserService">The currentUserService<see cref="ICurrentUserService"/></param>
        /// <param name="env">The env<see cref="IWebHostEnvironment"/></param>
        public StockController(
            IStockService stockService,
            ICurrentUserService currentUserService,
            IWebHostEnvironment env) : base(currentUserService)
        {
            _stockService = Guard.AgainstNull(stockService, nameof(stockService));
            _env = Guard.AgainstNull(env, nameof(env));
        }

        /// <summary>
        /// Serves the cached PNG logo for a symbol from wwwroot/logos.
        /// Anonymous so &lt;img&gt; tags do not need a JWT.
        /// </summary>
        [AllowAnonymous]
        [HttpGet(StockRoutes.Logo)]
        [ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Any)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Logo(string symbol)
        {
            symbol = (symbol ?? string.Empty).Trim().ToUpperInvariant();
            if (!Regex.IsMatch(symbol, "^[A-Z0-9]{1,24}$"))
                return NotFound();

            var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
            var path = Path.GetFullPath(Path.Combine(webRoot, "logos", $"{symbol}.png"));
            var logosRoot = Path.GetFullPath(Path.Combine(webRoot, "logos")) + Path.DirectorySeparatorChar;
            if (!path.StartsWith(logosRoot, StringComparison.OrdinalIgnoreCase) || !System.IO.File.Exists(path))
                return NotFound();

            return PhysicalFile(path, "image/png");
        }

        /// <summary>
        /// The Search
        /// </summary>
        /// <param name="request">The request<see cref="QueryRequest"/></param>
        /// <param name="cancellationToken">The cancellationToken<see cref="CancellationToken"/></param>
        /// <returns>The <see cref="Task{IActionResult}"/></returns>
        [HttpPost(StockRoutes.Search)]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<StocksResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Search([FromBody] QueryRequest request, CancellationToken cancellationToken)
        {
            var result = await _stockService.SearchAsync(request, cancellationToken);

            return Success(result);
        }

        /// <summary>
        /// The GetById
        /// </summary>
        /// <param name="id">The id<see cref="long"/></param>
        /// <param name="cancellationToken">The cancellationToken<see cref="CancellationToken"/></param>
        /// <returns>The <see cref="Task{IActionResult}"/></returns>
        [HttpGet(StockRoutes.GetById)]
        [ProducesResponseType(typeof(ApiResponse<StocksResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(long id, CancellationToken cancellationToken)
        {
            var result = await _stockService.GetByIdAsync(id, cancellationToken);

            if (result is null)
            {
                throw new NotFoundException("Stock not found.");
            }

            return Success(result);
        }

        /// <summary>
        /// The Create
        /// </summary>
        /// <param name="request">The request<see cref="StockCreateRequest"/></param>
        /// <param name="cancellationToken">The cancellationToken<see cref="CancellationToken"/></param>
        /// <returns>The <see cref="Task{IActionResult}"/></returns>
        [HttpPost(StockRoutes.Create)]
        [ProducesResponseType(typeof(ApiResponse<StocksResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create([FromBody] StockCreateRequest request, CancellationToken cancellationToken)
        {
            var result = await _stockService.CreateAsync(request, cancellationToken);

            return CreatedResponse(nameof(GetById), new { id = result.Data.Id }, result);
        }

        /// <summary>
        /// The Update
        /// </summary>
        /// <param name="id">The id<see cref="long"/></param>
        /// <param name="request">The request<see cref="StockUpdateRequest"/></param>
        /// <param name="cancellationToken">The cancellationToken<see cref="CancellationToken"/></param>
        /// <returns>The <see cref="Task{IActionResult}"/></returns>
        [HttpPut(StockRoutes.Update)]
        [ProducesResponseType(typeof(ApiResponse<StocksResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Update(long id, [FromBody] StockUpdateRequest request, CancellationToken cancellationToken)
        {
            var result = await _stockService.UpdateAsync(id, request, cancellationToken);

            return Success(result);
        }

        /// <summary>
        /// The Delete
        /// </summary>
        /// <param name="id">The id<see cref="long"/></param>
        /// <param name="cancellationToken">The cancellationToken<see cref="CancellationToken"/></param>
        /// <returns>The <see cref="Task{IActionResult}"/></returns>
        [HttpDelete(StockRoutes.Delete)]
        [ProducesResponseType(typeof(ApiResponse<StocksResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
        {
            var result = await _stockService.DeleteAsync(id, cancellationToken);

            return Success(result);
        }
    }
}
