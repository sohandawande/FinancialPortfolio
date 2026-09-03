using FinancialPortfolio.Api.Controllers.Base;
using FinancialPortfolio.Business.Abstractions.IWealth;
using FinancialPortfolio.Models.Abstractions.ICurrentUser;
using FinancialPortfolio.Models.Common.Utilities;
using FinancialPortfolio.Models.Model.Request.Wealth;
using Microsoft.AspNetCore.Mvc;

namespace FinancialPortfolio.Api.Controllers.Wealth
{
    [Route("api/[controller]")]
    public class WealthController : BaseApiController
    {
        private readonly IWealthTrackerService _wealth;
        private readonly IMutualFundNavService _nav;

        public WealthController(
            IWealthTrackerService wealth,
            IMutualFundNavService nav,
            ICurrentUserService currentUser)
            : base(currentUser)
        {
            _wealth = Guard.AgainstNull(wealth, nameof(wealth));
            _nav = Guard.AgainstNull(nav, nameof(nav));
        }

        [HttpGet("summary")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Summary(CancellationToken cancellationToken)
            => Ok(await _wealth.GetSummaryAsync(cancellationToken));

        [HttpGet("mutual-funds/search")]
        public async Task<IActionResult> SearchSchemes([FromQuery] string q, CancellationToken cancellationToken)
            => Ok(await _nav.SearchAsync(q ?? string.Empty, cancellationToken));

        [HttpPost("mutual-funds/sync-nav")]
        public async Task<IActionResult> SyncAllNav(CancellationToken cancellationToken)
            => Ok(await _nav.SyncPortfolioNavAsync(cancellationToken));

        [HttpPost("mutual-funds/{id:long}/sync-nav")]
        public async Task<IActionResult> SyncOneNav(long id, CancellationToken cancellationToken)
            => Ok(await _nav.SyncOneAsync(id, cancellationToken));

        [HttpGet("mutual-funds")]
        public async Task<IActionResult> MutualFunds(CancellationToken cancellationToken)
            => Ok(await _wealth.GetMutualFundsAsync(cancellationToken));

        [HttpPost("mutual-funds")]
        public async Task<IActionResult> AddMutualFund([FromBody] UpsertMutualFundRequest request, CancellationToken cancellationToken)
            => Ok(await _wealth.AddMutualFundAsync(request, cancellationToken));

        [HttpPut("mutual-funds/{id:long}")]
        public async Task<IActionResult> UpdateMutualFund(long id, [FromBody] UpsertMutualFundRequest request, CancellationToken cancellationToken)
            => Ok(await _wealth.UpdateMutualFundAsync(id, request, cancellationToken));

        [HttpDelete("mutual-funds/{id:long}")]
        public async Task<IActionResult> DeleteMutualFund(long id, CancellationToken cancellationToken)
            => Ok(await _wealth.DeleteMutualFundAsync(id, cancellationToken));

        [HttpGet("fixed-deposits")]
        public async Task<IActionResult> FixedDeposits(CancellationToken cancellationToken)
            => Ok(await _wealth.GetFixedDepositsAsync(cancellationToken));

        [HttpPost("fixed-deposits")]
        public async Task<IActionResult> AddFixedDeposit([FromBody] UpsertFixedDepositRequest request, CancellationToken cancellationToken)
            => Ok(await _wealth.AddFixedDepositAsync(request, cancellationToken));

        [HttpPut("fixed-deposits/{id:long}")]
        public async Task<IActionResult> UpdateFixedDeposit(long id, [FromBody] UpsertFixedDepositRequest request, CancellationToken cancellationToken)
            => Ok(await _wealth.UpdateFixedDepositAsync(id, request, cancellationToken));

        [HttpDelete("fixed-deposits/{id:long}")]
        public async Task<IActionResult> DeleteFixedDeposit(long id, CancellationToken cancellationToken)
            => Ok(await _wealth.DeleteFixedDepositAsync(id, cancellationToken));

        [HttpGet("recurring-deposits")]
        public async Task<IActionResult> RecurringDeposits(CancellationToken cancellationToken)
            => Ok(await _wealth.GetRecurringDepositsAsync(cancellationToken));

        [HttpGet("recurring-deposits/{id:long}")]
        public async Task<IActionResult> GetRecurringDepositDetail(long id, CancellationToken cancellationToken)
            => Ok(await _wealth.GetRecurringDepositDetailAsync(id, cancellationToken));

        [HttpPost("recurring-deposits")]
        public async Task<IActionResult> AddRecurringDeposit([FromBody] UpsertRecurringDepositRequest request, CancellationToken cancellationToken)
            => Ok(await _wealth.AddRecurringDepositAsync(request, cancellationToken));

        [HttpPut("recurring-deposits/{id:long}")]
        public async Task<IActionResult> UpdateRecurringDeposit(long id, [FromBody] UpsertRecurringDepositRequest request, CancellationToken cancellationToken)
            => Ok(await _wealth.UpdateRecurringDepositAsync(id, request, cancellationToken));

        [HttpDelete("recurring-deposits/{id:long}")]
        public async Task<IActionResult> DeleteRecurringDeposit(long id, CancellationToken cancellationToken)
            => Ok(await _wealth.DeleteRecurringDepositAsync(id, cancellationToken));

        [HttpGet("recurring-deposits/{id:long}/installments")]
        public async Task<IActionResult> GetRdInstallments(long id, CancellationToken cancellationToken)
            => Ok(await _wealth.GetRdInstallmentsAsync(id, cancellationToken));

        [HttpPost("recurring-deposits/{id:long}/pay")]
        public async Task<IActionResult> PayRdInstallment(long id, [FromBody] PayRdInstallmentRequest request, CancellationToken cancellationToken)
            => Ok(await _wealth.PayRdInstallmentAsync(id, request, cancellationToken));

        [HttpPost("recurring-deposits/{id:long}/installments")]
        public async Task<IActionResult> UpsertRdInstallment(long id, [FromBody] UpsertRdInstallmentRequest request, CancellationToken cancellationToken)
            => Ok(await _wealth.UpsertRdInstallmentAsync(id, request, cancellationToken));

        [HttpDelete("recurring-deposits/{id:long}/installments/{installmentId:long}")]
        public async Task<IActionResult> DeleteRdInstallment(long id, long installmentId, CancellationToken cancellationToken)
            => Ok(await _wealth.DeleteRdInstallmentAsync(id, installmentId, cancellationToken));

        [HttpGet("insurance-policies")]
        public async Task<IActionResult> InsurancePolicies(CancellationToken cancellationToken)
            => Ok(await _wealth.GetInsurancePoliciesAsync(cancellationToken));

        [HttpPost("insurance-policies")]
        public async Task<IActionResult> AddInsurancePolicy([FromBody] UpsertInsurancePolicyRequest request, CancellationToken cancellationToken)
            => Ok(await _wealth.AddInsurancePolicyAsync(request, cancellationToken));

        [HttpPut("insurance-policies/{id:long}")]
        public async Task<IActionResult> UpdateInsurancePolicy(long id, [FromBody] UpsertInsurancePolicyRequest request, CancellationToken cancellationToken)
            => Ok(await _wealth.UpdateInsurancePolicyAsync(id, request, cancellationToken));

        [HttpDelete("insurance-policies/{id:long}")]
        public async Task<IActionResult> DeleteInsurancePolicy(long id, CancellationToken cancellationToken)
            => Ok(await _wealth.DeleteInsurancePolicyAsync(id, cancellationToken));
    }
}
