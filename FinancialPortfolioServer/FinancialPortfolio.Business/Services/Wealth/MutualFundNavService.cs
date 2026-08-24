using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using FinancialPortfolio.Business.Abstractions.ILogger;
using FinancialPortfolio.Business.Abstractions.IWealth;
using FinancialPortfolio.Business.Common.Helpers;
using FinancialPortfolio.Data.Context;
using FinancialPortfolio.Models.Abstractions.ICurrentUser;
using FinancialPortfolio.Models.Common.Exceptions;
using FinancialPortfolio.Models.Common.Response;
using FinancialPortfolio.Models.Common.Utilities;
using FinancialPortfolio.Models.Model.Response.Wealth;
using FinancialPortfolio.Models.Model.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FinancialPortfolio.Business.Services.Wealth
{
    public sealed class MutualFundNavService : IMutualFundNavService
    {
        private readonly HttpClient _http;
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IApplicationLoggerService _logger;
        private readonly MutualFundNavSettings _settings;

        public MutualFundNavService(
            HttpClient http,
            ApplicationDbContext context,
            ICurrentUserService currentUser,
            IApplicationLoggerService logger,
            IOptions<MutualFundNavSettings> options)
        {
            _http = Guard.AgainstNull(http, nameof(http));
            _context = Guard.AgainstNull(context, nameof(context));
            _currentUser = Guard.AgainstNull(currentUser, nameof(currentUser));
            _logger = Guard.AgainstNull(logger, nameof(logger));
            _settings = options.Value;
        }

        public async Task<ApiResponse<List<MutualFundSchemeLookupResponse>>> SearchAsync(string query, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Trim().Length < 2)
                return ResponseFactory.Success(new List<MutualFundSchemeLookupResponse>(), "Type at least 2 characters.");

            try
            {
                var url = $"{_settings.MfApiBaseUrl.TrimEnd('/')}/mf/search?q={Uri.EscapeDataString(query.Trim())}";
                using var response = await _http.GetAsync(url, cancellationToken);
                if (!response.IsSuccessStatusCode)
                    throw new InvalidOperationException($"mfapi search failed ({(int)response.StatusCode}). Try again.");

                var rows = await response.Content.ReadFromJsonAsync<List<MfApiSearchItem>>(cancellationToken: cancellationToken)
                           ?? [];

                var result = rows
                    .Where(x => x.SchemeCode > 0 && !string.IsNullOrWhiteSpace(x.SchemeName))
                    .Take(25)
                    .Select(x => new MutualFundSchemeLookupResponse
                    {
                        SchemeCode = x.SchemeCode,
                        SchemeName = x.SchemeName.Trim(),
                        Source = "mfapi"
                    })
                    .ToList();

                return ResponseFactory.Success(result, result.Count == 0 ? "No schemes matched." : "Schemes fetched.");
            }
            catch (Exception ex) when (ex is not InvalidOperationException)
            {
                throw new InvalidOperationException("Could not search schemes. Check internet and try again.", ex);
            }
        }

        public async Task<ApiResponse<MutualFundNavSyncResponse>> SyncPortfolioNavAsync(CancellationToken cancellationToken)
        {
            var portfolio = await _context.Portfolios.AsNoTracking()
                .FirstOrDefaultAsync(p => p.UserId == _currentUser.UserId && p.IsActive, cancellationToken);
            if (portfolio is null)
                return ResponseFactory.Success(new MutualFundNavSyncResponse { Skipped = 0 }, "No portfolio created yet.");

            var funds = await _context.PortfolioMutualFunds
                .Where(x => x.PortfolioId == portfolio.Id && x.IsActive && x.SchemeCode != null)
                .ToListAsync(cancellationToken);

            return ResponseFactory.Success(await SyncRowsAsync(funds, cancellationToken), "NAV sync finished.");
        }

        public async Task<ApiResponse<MutualFundNavSyncResponse>> SyncOneAsync(long mutualFundId, CancellationToken cancellationToken)
        {
            var portfolio = await _context.Portfolios.AsNoTracking()
                .FirstOrDefaultAsync(p => p.UserId == _currentUser.UserId && p.IsActive, cancellationToken)
                ?? throw new NotFoundException("Create a portfolio first.");

            var fund = await _context.PortfolioMutualFunds
                .FirstOrDefaultAsync(x => x.Id == mutualFundId && x.PortfolioId == portfolio.Id, cancellationToken)
                ?? throw new NotFoundException("Mutual fund not found.");

            if (fund.SchemeCode is null)
                throw new InvalidOperationException("This scheme has no scheme code. Search and pick a scheme first.");

            return ResponseFactory.Success(await SyncRowsAsync([fund], cancellationToken), "NAV updated.");
        }

        public async Task<MutualFundNavSyncResponse> SyncAllActiveAsync(CancellationToken cancellationToken)
        {
            var funds = await _context.PortfolioMutualFunds
                .Where(x => x.IsActive && x.SchemeCode != null)
                .ToListAsync(cancellationToken);
            return await SyncRowsAsync(funds, cancellationToken);
        }

        private async Task<MutualFundNavSyncResponse> SyncRowsAsync(
            List<Data.Entities.Portfolio.PortfolioMutualFundEntity> funds,
            CancellationToken cancellationToken)
        {
            var result = new MutualFundNavSyncResponse();
            if (funds.Count == 0)
            {
                result.Skipped = 0;
                return result;
            }

            Dictionary<int, MutualFundNavQuote>? amfiCache = null;

            foreach (var fund in funds)
            {
                if (fund.SchemeCode is null)
                {
                    result.Skipped++;
                    continue;
                }

                try
                {
                    var quote = await TryMfApiAsync(fund.SchemeCode.Value, cancellationToken);
                    if (quote is null)
                    {
                        amfiCache ??= await TryLoadAmfiAsync(cancellationToken);
                        amfiCache.TryGetValue(fund.SchemeCode.Value, out quote);
                    }

                    if (quote is null || quote.Nav <= 0)
                    {
                        result.Failed++;
                        result.Errors.Add($"{fund.SchemeName}: NAV not found on mfapi or AMFI.");
                        continue;
                    }

                    fund.CurrentNav = quote.Nav;
                    fund.NavAsOf = quote.AsOf;
                    fund.NavSource = quote.Source;
                    if (string.IsNullOrWhiteSpace(fund.Amc) && !string.IsNullOrWhiteSpace(quote.Amc))
                        fund.Amc = quote.Amc;
                    result.Updated++;
                    result.PrimarySource = quote.Source;
                }
                catch (Exception ex)
                {
                    result.Failed++;
                    result.Errors.Add($"{fund.SchemeName}: {ex.Message}");
                }
            }

            if (result.Updated > 0)
                await _context.SaveChangesAsync(cancellationToken);

            return result;
        }

        private async Task<MutualFundNavQuote?> TryMfApiAsync(int schemeCode, CancellationToken cancellationToken)
        {
            try
            {
                var url = $"{_settings.MfApiBaseUrl.TrimEnd('/')}/mf/{schemeCode}";
                using var response = await _http.GetAsync(url, cancellationToken);
                if (!response.IsSuccessStatusCode) return null;

                using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
                var root = doc.RootElement;
                var meta = root.TryGetProperty("meta", out var m) ? m : default;
                var data = root.TryGetProperty("data", out var d) ? d : default;
                if (data.ValueKind != JsonValueKind.Array || data.GetArrayLength() == 0) return null;

                var first = data[0];
                if (!decimal.TryParse(first.GetProperty("nav").GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var nav) || nav <= 0)
                    return null;

                DateTime asOf = DateTime.UtcNow.Date;
                if (DateTime.TryParseExact(first.GetProperty("date").GetString(), "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
                    asOf = parsed;

                return new MutualFundNavQuote
                {
                    SchemeCode = schemeCode,
                    SchemeName = meta.ValueKind == JsonValueKind.Object && meta.TryGetProperty("scheme_name", out var n) ? n.GetString() ?? "" : "",
                    Amc = meta.ValueKind == JsonValueKind.Object && meta.TryGetProperty("fund_house", out var h) ? h.GetString() : null,
                    Nav = DepositMath.Round(nav),
                    AsOf = asOf,
                    Source = "mfapi"
                };
            }
            catch
            {
                return null;
            }
        }

        private async Task<Dictionary<int, MutualFundNavQuote>> TryLoadAmfiAsync(CancellationToken cancellationToken)
        {
            var map = new Dictionary<int, MutualFundNavQuote>();
            try
            {
                var text = await _http.GetStringAsync(_settings.AmfiNavUrl, cancellationToken);
                foreach (var raw in text.Split('\n'))
                {
                    var line = raw.Trim();
                    if (string.IsNullOrWhiteSpace(line) || !char.IsDigit(line[0])) continue;
                    var parts = line.Split(';');
                    if (parts.Length < 5) continue;
                    if (!int.TryParse(parts[0].Trim(), out var code)) continue;
                    if (!decimal.TryParse(parts[4].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var nav) || nav <= 0) continue;

                    DateTime asOf = DateTime.UtcNow.Date;
                    if (parts.Length > 5)
                        DateTime.TryParse(parts[5].Trim(), CultureInfo.GetCultureInfo("en-IN"), DateTimeStyles.None, out asOf);

                    map[code] = new MutualFundNavQuote
                    {
                        SchemeCode = code,
                        SchemeName = parts[3].Trim(),
                        Nav = DepositMath.Round(nav),
                        AsOf = asOf,
                        Source = "amfi"
                    };
                }
            }
            catch
            {
                // fallback dump is best-effort
            }

            return map;
        }

        private sealed class MfApiSearchItem
        {
            public int SchemeCode { get; set; }
            public string SchemeName { get; set; } = string.Empty;
        }
    }
}
