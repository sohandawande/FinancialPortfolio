namespace FinancialPortfolio.Api.Controllers.Etf
{
    using FinancialPortfolio.Api.Common.Constants.Routes;
    using FinancialPortfolio.Api.Controllers.Base;
    using FinancialPortfolio.Business.Abstractions.IEtf;
    using FinancialPortfolio.Models.Abstractions.ICurrentUser;
    using FinancialPortfolio.Models.Common.Exceptions;
    using FinancialPortfolio.Models.Common.Response;
    using FinancialPortfolio.Models.Common.Utilities;
    using FinancialPortfolio.Models.Model.Request.Etf;
    using FinancialPortfolio.Models.Model.Response.Etf;
    using FinancialPortfolio.QueryEngine.Models;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Defines the <see cref="EtfController" />
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    public sealed class EtfController : BaseApiController
    {
        /// <summary>
        /// Defines the _stockService
        /// </summary>
        private readonly IEtfService _stockService;
        private readonly IWebHostEnvironment _env;

        /// <summary>
        /// Initializes a new instance of the <see cref="EtfController"/> class.
        /// </summary>
        /// <param name="stockService">The stockService<see cref="IEtfService"/></param>
        /// <param name="currentUserService">The currentUserService<see cref="ICurrentUserService"/></param>
        /// <param name="env">The env<see cref="IWebHostEnvironment"/></param>
        public EtfController(
            IEtfService stockService,
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
        [HttpGet(EtfRoutes.Logo)]
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
        [HttpPost(EtfRoutes.Search)]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<EtfsResponse>>), StatusCodes.Status200OK)]
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
        [HttpGet(EtfRoutes.GetById)]
        [ProducesResponseType(typeof(ApiResponse<EtfsResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(long id, CancellationToken cancellationToken)
        {
            var result = await _stockService.GetByIdAsync(id, cancellationToken);

            if (result is null)
            {
                throw new NotFoundException("ETF not found.");
            }

            return Success(result);
        }

        /// <summary>
        /// The Create
        /// </summary>
        /// <param name="request">The request<see cref="EtfCreateRequest"/></param>
        /// <param name="cancellationToken">The cancellationToken<see cref="CancellationToken"/></param>
        /// <returns>The <see cref="Task{IActionResult}"/></returns>
        [HttpPost(EtfRoutes.Create)]
        [ProducesResponseType(typeof(ApiResponse<EtfsResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create([FromBody] EtfCreateRequest request, CancellationToken cancellationToken)
        {
            var result = await _stockService.CreateAsync(request, cancellationToken);

            return CreatedResponse(nameof(GetById), new { id = result.Data.Id }, result);
        }

        /// <summary>
        /// The Update
        /// </summary>
        /// <param name="id">The id<see cref="long"/></param>
        /// <param name="request">The request<see cref="EtfUpdateRequest"/></param>
        /// <param name="cancellationToken">The cancellationToken<see cref="CancellationToken"/></param>
        /// <returns>The <see cref="Task{IActionResult}"/></returns>
        [HttpPut(EtfRoutes.Update)]
        [ProducesResponseType(typeof(ApiResponse<EtfsResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Update(long id, [FromBody] EtfUpdateRequest request, CancellationToken cancellationToken)
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
        [HttpDelete(EtfRoutes.Delete)]
        [ProducesResponseType(typeof(ApiResponse<EtfsResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
        {
            var result = await _stockService.DeleteAsync(id, cancellationToken);

            return Success(result);
        }
    }
}
