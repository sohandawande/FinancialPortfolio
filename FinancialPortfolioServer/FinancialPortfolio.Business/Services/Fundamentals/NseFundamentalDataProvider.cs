using FinancialPortfolio.Business.Abstractions.IFundamentals;
using FinancialPortfolio.Models.Common.Utilities;
using FinancialPortfolio.Models.Model.Settings;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FinancialPortfolio.Business.Services.Fundamentals
{
    /// <summary>
    /// Reads stock valuation data exclusively from NSE's equity quote API.
    /// The daily PR archive remains useful for historical market data, while
    /// stock P/E is read from the equity quote metadata (not the NSE index PE report).
    /// </summary>
    public sealed class NseFundamentalDataProvider : IFundamentalDataProvider
    {
        private const string SourceName = "NSE";
        private const int MaxConcurrency = 8;
        private readonly HttpClient _httpClient;
        private readonly FundamentalDataSettings _settings;
        private readonly SemaphoreSlim _sessionLock = new(1, 1);
        private bool _sessionInitialized;

        public NseFundamentalDataProvider(
            HttpClient httpClient,
            IOptions<FundamentalDataSettings> options)
        {
            _httpClient = Guard.AgainstNull(httpClient, nameof(httpClient));
            _settings = Guard.AgainstNull(options.Value, nameof(options));
        }

        public async Task<IReadOnlyList<StockFundamentalData>> GetAsync(
            IReadOnlyCollection<string> symbols,
            DateOnly tradeDate,
            CancellationToken cancellationToken = default)
        {
            if (!_settings.Enabled || symbols.Count == 0)
                return [];

            var requestedSymbols = symbols
                .Where(static x => !string.IsNullOrWhiteSpace(x))
                .Select(NormalizeSymbol)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (requestedSymbols.Length == 0)
                return [];

            // NSE's equity quote contains the current Symbol P/E. The old PE_{date}.csv
            // report is an INDEX P/E report and must never be used for stock P/E.
            await EnsureNseSessionAsync(cancellationToken);

            var results = new System.Collections.Concurrent.ConcurrentBag<StockFundamentalData>();
            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = MaxConcurrency,
                CancellationToken = cancellationToken
            };

            await Parallel.ForEachAsync(requestedSymbols, options, async (symbol, token) =>
            {
                var quote = await GetQuoteAsync(symbol, token);
                if (quote is null)
                    return;

                decimal? pe = quote.Metadata?.SymbolPe > 0 ? quote.Metadata.SymbolPe : null;
                decimal? lastPrice = quote.PriceInfo?.LastPrice > 0 ? quote.PriceInfo.LastPrice : null;
                decimal? issuedSize = quote.SecurityInfo?.IssuedSize > 0 ? quote.SecurityInfo.IssuedSize : null;

                // IssuedSize is the number of equity shares. Converting rupees to crore:
                // shares * price / 10,000,000.
                var marketCapCrore = issuedSize.HasValue && lastPrice.HasValue
                    ? Math.Round(issuedSize.Value * lastPrice.Value / 10_000_000m, 5)
                    : (decimal?)null;

                results.Add(new StockFundamentalData
                {
                    Symbol = symbol,
                    MarketCapCrore = marketCapCrore is > 0 ? marketCapCrore : null,
                    PE = pe,
                    // NSE quote metadata exposes Symbol P/E, not a standalone TTM EPS field.
                    // The enrichment layer derives EPS as CurrentPrice / Symbol P/E.
                    EPS = null,
                    RetrievedAt = DateTimeOffset.UtcNow,
                    Source = SourceName
                });
            });

            return results
                .OrderBy(x => x.Symbol, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        private async Task<NseEquityQuote?> GetQuoteAsync(
            string symbol,
            CancellationToken cancellationToken)
        {
            var encodedSymbol = Uri.EscapeDataString(symbol);
            var url = $"https://www.nseindia.com/api/quote-equity?symbol={encodedSymbol}";

            for (var attempt = 0; attempt < 2; attempt++)
            {
                try
                {
                    using var response = await _httpClient.GetAsync(url, cancellationToken);
                    if (response.StatusCode == System.Net.HttpStatusCode.Forbidden
                        || response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(500 * (attempt + 1)), cancellationToken);
                        continue;
                    }

                    if (!response.IsSuccessStatusCode)
                        return null;

                    await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                    return await JsonSerializer.DeserializeAsync<NseEquityQuote>(
                        stream,
                        NseJsonOptions,
                        cancellationToken);
                }
                catch (JsonException)
                {
                    return null;
                }
                catch (HttpRequestException) when (attempt == 0)
                {
                    await Task.Delay(250, cancellationToken);
                }
                catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested && attempt == 0)
                {
                    await Task.Delay(250, cancellationToken);
                }
            }

            return null;
        }

        private async Task EnsureNseSessionAsync(CancellationToken cancellationToken)
        {
            if (_sessionInitialized)
                return;

            await _sessionLock.WaitAsync(cancellationToken);
            try
            {
                if (_sessionInitialized)
                    return;

                using var response = await _httpClient.GetAsync(
                    "https://www.nseindia.com/",
                    cancellationToken);

                // Even if the home page is temporarily unavailable, the quote request
                // can still work. Do not fail the entire synchronization here.
                _sessionInitialized = true;
            }
            catch (HttpRequestException)
            {
                _sessionInitialized = true;
            }
            finally
            {
                _sessionLock.Release();
            }
        }

        private static string NormalizeSymbol(string symbol)
            => symbol.Trim().ToUpperInvariant().Replace(".NS", string.Empty, StringComparison.OrdinalIgnoreCase);

        private static readonly JsonSerializerOptions NseJsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private sealed class NseEquityQuote
        {
            [JsonPropertyName("metadata")]
            public NseMetadata? Metadata { get; set; }

            [JsonPropertyName("priceInfo")]
            public NsePriceInfo? PriceInfo { get; set; }

            [JsonPropertyName("securityInfo")]
            public NseSecurityInfo? SecurityInfo { get; set; }
        }

        private sealed class NseMetadata
        {
            [JsonPropertyName("pdSymbolPe")]
            public decimal SymbolPe { get; set; }
        }

        private sealed class NsePriceInfo
        {
            [JsonPropertyName("lastPrice")]
            public decimal LastPrice { get; set; }
        }

        private sealed class NseSecurityInfo
        {
            [JsonPropertyName("issuedSize")]
            public decimal IssuedSize { get; set; }
        }
    }
}
