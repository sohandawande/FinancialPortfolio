using System.Net.Http.Json;
using FinancialPortfolio.Business.Abstractions.IWealth;
using FinancialPortfolio.Models.Common.Response;
using FinancialPortfolio.Models.Common.Utilities;
using FinancialPortfolio.Models.Model.Response.Wealth;

namespace FinancialPortfolio.Business.Services.Wealth
{
    /// <summary>
    /// Free IFSC lookup (Razorpay public API) + static bank-name suggestions.
    /// Same style as MutualFundNavService (HttpClient injected).
    /// </summary>
    public sealed class BankLookupService : IBankLookupService
    {
        private readonly HttpClient _http;

        private static readonly (string Name, string Code)[] PopularBanks =
        [
            ("State Bank of India", "SBIN"),
            ("HDFC Bank", "HDFC"),
            ("ICICI Bank", "ICIC"),
            ("Axis Bank", "UTIB"),
            ("Punjab National Bank", "PUNB"),
            ("Bank of Baroda", "BARB"),
            ("Canara Bank", "CNRB"),
            ("Union Bank of India", "UBIN"),
            ("Bank of India", "BKID"),
            ("Indian Bank", "IDIB"),
            ("Indian Overseas Bank", "IOBA"),
            ("Central Bank of India", "CBIN"),
            ("UCO Bank", "UCBA"),
            ("Bank of Maharashtra", "MAHB"),
            ("Punjab & Sind Bank", "PSIB"),
            ("Kotak Mahindra Bank", "KKBK"),
            ("Yes Bank", "YESB"),
            ("IDFC FIRST Bank", "IDFB"),
            ("IndusInd Bank", "INDB"),
            ("Federal Bank", "FDRL"),
            ("South Indian Bank", "SIBL"),
            ("RBL Bank", "RATN"),
            ("Bandhan Bank", "BDBL"),
            ("AU Small Finance Bank", "AUBL"),
            ("IDBI Bank", "IBKL"),
            ("Jammu & Kashmir Bank", "JAKA"),
            ("Karnataka Bank", "KARB"),
            ("Karur Vysya Bank", "KVBL"),
            ("City Union Bank", "CIUB")
        ];

        public BankLookupService(HttpClient http)
        {
            _http = Guard.AgainstNull(http, nameof(http));
            if (_http.BaseAddress is null)
                _http.BaseAddress = new Uri("https://ifsc.razorpay.com/");
            _http.Timeout = TimeSpan.FromSeconds(8);
        }

        public async Task<ApiResponse<BankIfscResponse?>> LookupIfscAsync(string ifsc, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(ifsc) || ifsc.Trim().Length != 11)
                return ResponseFactory.Success<BankIfscResponse?>(null, "Invalid IFSC.");

            var code = ifsc.Trim().ToUpperInvariant();
            try
            {
                using var response = await _http.GetAsync(code, cancellationToken);
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return ResponseFactory.Success<BankIfscResponse?>(null, "IFSC not found.");

                if (!response.IsSuccessStatusCode)
                    return ResponseFactory.Failure<BankIfscResponse?>("IFSC lookup failed.");

                var dto = await response.Content.ReadFromJsonAsync<RazorpayIfscDto>(cancellationToken: cancellationToken);
                if (dto is null)
                    return ResponseFactory.Success<BankIfscResponse?>(null, "Empty response.");

                return ResponseFactory.Success<BankIfscResponse?>(new BankIfscResponse
                {
                    Ifsc = dto.IFSC ?? code,
                    Bank = dto.BANK ?? string.Empty,
                    BankCode = dto.BANKCODE,
                    Branch = dto.BRANCH,
                    Address = dto.ADDRESS,
                    City = dto.CITY,
                    District = dto.DISTRICT,
                    State = dto.STATE,
                    Contact = dto.CONTACT,
                    Micr = dto.MICR,
                    Rtgs = dto.RTGS,
                    Neft = dto.NEFT,
                    Imps = dto.IMPS,
                    Upi = dto.UPI
                }, "IFSC resolved.");
            }
            catch (Exception ex)
            {
                return ResponseFactory.Failure<BankIfscResponse?>("IFSC lookup failed.", ex.Message);
            }
        }

        public Task<ApiResponse<List<BankSuggestionResponse>>> SearchBanksAsync(string query, CancellationToken cancellationToken)
        {
            var q = (query ?? string.Empty).Trim();
            IEnumerable<(string Name, string Code)> source = PopularBanks;

            if (!string.IsNullOrWhiteSpace(q))
            {
                source = PopularBanks.Where(b =>
                    b.Name.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    b.Code.Contains(q, StringComparison.OrdinalIgnoreCase));
            }

            var list = source
                .Take(20)
                .Select(b => new BankSuggestionResponse { Name = b.Name, Code = b.Code })
                .ToList();

            return Task.FromResult(ResponseFactory.Success(list, "Bank suggestions."));
        }

        private sealed class RazorpayIfscDto
        {
            public string? BANK { get; set; }
            public string? IFSC { get; set; }
            public string? BRANCH { get; set; }
            public string? ADDRESS { get; set; }
            public string? CITY { get; set; }
            public string? DISTRICT { get; set; }
            public string? STATE { get; set; }
            public string? CONTACT { get; set; }
            public string? MICR { get; set; }
            public string? BANKCODE { get; set; }
            public bool? RTGS { get; set; }
            public bool? NEFT { get; set; }
            public bool? IMPS { get; set; }
            public bool? UPI { get; set; }
        }
    }
}
