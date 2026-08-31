using FinancialPortfolio.Api.Controllers.Base;
using FinancialPortfolio.Business.Abstractions.IWealth;
using FinancialPortfolio.Models.Abstractions.ICurrentUser;
using FinancialPortfolio.Models.Common.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace FinancialPortfolio.Api.Controllers.Bank
{
    [Route("api/[controller]")]
    public class BankController : BaseApiController
    {
        private readonly IBankLookupService _banks;

        public BankController(IBankLookupService banks, ICurrentUserService currentUser)
            : base(currentUser)
        {
            _banks = Guard.AgainstNull(banks, nameof(banks));
        }

        /// <summary>GET /api/bank/ifsc/SBIN0000001</summary>
        [HttpGet("ifsc/{ifsc}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> LookupIfsc(string ifsc, CancellationToken cancellationToken)
            => Ok(await _banks.LookupIfscAsync(ifsc, cancellationToken));

        /// <summary>GET /api/bank/search?q=hdfc</summary>
        [HttpGet("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Search([FromQuery] string q, CancellationToken cancellationToken)
            => Ok(await _banks.SearchBanksAsync(q ?? string.Empty, cancellationToken));
    }
}
