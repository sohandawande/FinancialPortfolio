using FinancialPortfolio.Business.Abstractions.IGoogleSheet;
using FinancialPortfolio.Business.Abstractions.ILogger;
using FinancialPortfolio.Business.Common.Helpers;
using FinancialPortfolio.Business.Common.Mapper;
using FinancialPortfolio.Business.Common.Urls;
using FinancialPortfolio.Models.Common.Enums;
using FinancialPortfolio.Models.Common.Exceptions;
using FinancialPortfolio.Models.Common.Response;
using FinancialPortfolio.Models.Common.Utilities;
using FinancialPortfolio.Models.Model.Request.GoogleSheet;
using FinancialPortfolio.Models.Model.Response.GoogleSheet;
using FinancialPortfolio.Models.Model.Settings;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace FinancialPortfolio.Business.Services.GoogleSheet
{
    public sealed class GoogleSheetService : IGoogleSheetService
    {
        private readonly HttpClient _httpClient;
        private readonly GoogleSheetSettings _settings;
        private readonly IApplicationLoggerService _logger;
        private readonly IGoogleSheetSyncService _syncService;
        private readonly TimeProvider _timeProvider;

        public GoogleSheetService(
            IHttpClientFactory httpClientFactory,
            IOptions<GoogleSheetSettings> options,
            IApplicationLoggerService logger,
            IGoogleSheetSyncService syncService,
            TimeProvider timeProvider)
        {
            _httpClient = httpClientFactory.CreateClient("GoogleSheet");
            _settings = Guard.AgainstNull(options.Value, nameof(options));
            _logger = Guard.AgainstNull(logger, nameof(logger));
            _syncService = Guard.AgainstNull(syncService, nameof(syncService));
            _timeProvider = Guard.AgainstNull(timeProvider, nameof(timeProvider));
        }

        public async Task<ApiResponse<GoogleSheetSyncResponse>> SyncStocksAsync(GoogleSheetRequest request, CancellationToken cancellationToken = default)
        {
            Guard.AgainstNull(request, nameof(request));

            try
            {
                var url = GoogleSheetUrlBuilder.BuildReadUrl(_settings);

                var response = await _httpClient.GetAsync(url, cancellationToken);
                response.EnsureSuccessStatusCode();

                var sheet = await response.Content.ReadFromJsonAsync<GoogleSheetApiResponse>(cancellationToken: cancellationToken);

                if (sheet is null)
                {
                    await _logger.LogWarningAsync(ApplicationLevelType.Business, LogSourceHelper.Current(), "Google Sheet response is null.", cancellationToken);
                    throw new InvalidOperationException("Unable to read Google Sheet response.");
                }

                if (sheet.Values is null || sheet.Values.Count <= 1)
                {
                    await _logger.LogWarningAsync(ApplicationLevelType.Business, LogSourceHelper.Current(), "No data found in Google Sheet.", cancellationToken);
                    throw new NotFoundException("No Data Found");
                }

                await _logger.LogInformationAsync(ApplicationLevelType.Business, LogSourceHelper.Current(), $"Google Sheet fetched successfully with {sheet.Values.Count - 1} records.", cancellationToken);

                var (items, skipped) = MapSheetToResponses(sheet.Values);

                var syncResult = await _syncService.SyncAsync(items, cancellationToken);
                syncResult.SkippedRecords = skipped;

                return ResponseFactory.Success(syncResult, "Google Sheet synchronized successfully.");
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ApplicationLevelType.Business, LogSourceHelper.Current(), ex.Message, ex, cancellationToken);
                throw;
            }
        }

        private static (List<GoogleSheetResponse> Items, int Skipped) MapSheetToResponses(List<List<object>> values)
        {
            var headerRow = values[0].Select(x => x?.ToString()?.Trim() ?? string.Empty).ToList();

            var columns = GoogleSheetColumnMapper.Build(headerRow);
            var result = new List<GoogleSheetResponse>();
            int skipped = 0;

            for (int i = 1; i < values.Count; i++)
            {
                var row = values[i];

                var symbol = GetFirstNonEmpty(row, columns, "Symbol");
                if (string.IsNullOrWhiteSpace(symbol))
                {
                    skipped++;
                    continue;
                }

                var item = new GoogleSheetResponse
                {
                    Symbol = symbol,
                    CompanyName = GetFirstNonEmpty(row, columns, "CompanyName", "Company Name", "Name"),
                    Industry = GetFirstNonEmpty(row, columns, "Industry"),
                    ISINCode = GetFirstNonEmpty(row, columns, "ISINCode", "ISIN Code"),
                    Series = GetFirstNonEmpty(row, columns, "Series"),
                    Category = GetFirstNonEmpty(row, columns, "Category"),
                    CurrentPrice = GoogleSheetHelper.ParseDecimal(GetFirstNonEmpty(row, columns, "CurrentPrice", "Current Price", "LTP")),
                    PreviousClose = GoogleSheetHelper.ParseDecimal(GetFirstNonEmpty(row, columns, "PreviousClose", "Previous Close")),
                    OpenPrice = GoogleSheetHelper.ParseDecimal(GetFirstNonEmpty(row, columns, "OpenPrice", "Open Price")),
                    HighPrice = GoogleSheetHelper.ParseDecimal(GetFirstNonEmpty(row, columns, "HighPrice", "High Price")),
                    LowPrice = GoogleSheetHelper.ParseDecimal(GetFirstNonEmpty(row, columns, "LowPrice", "Low Price")),
                    Volume = GoogleSheetHelper.ParseLong(GetFirstNonEmpty(row, columns, "Volume")),
                    AverageVolume = GoogleSheetHelper.ParseLong(GetFirstNonEmpty(row, columns, "AverageVolume")),
                    Week52High = GoogleSheetHelper.ParseDecimal(GetFirstNonEmpty(row, columns, "Week52High")),
                    Week52Low = GoogleSheetHelper.ParseDecimal(GetFirstNonEmpty(row, columns, "Week52Low")),
                    PE = GoogleSheetHelper.ParseDecimal(GetFirstNonEmpty(row, columns, "PE", "P/E", "PE Ratio", "P/E Ratio")),
                    EPS = GoogleSheetHelper.ParseDecimal(GetFirstNonEmpty(row, columns, "EPS")),
                    MarketCap = GoogleSheetHelper.ParseDecimal(GetFirstNonEmpty(row, columns, "MarketCap", "Market Cap")),
                    PriceChange = GoogleSheetHelper.ParseDecimal(GetFirstNonEmpty(row, columns, "PriceChange", "Price Change")),
                    IsActive = true,
                    LastUpdated = GoogleSheetHelper.ParseDateTime(GetFirstNonEmpty(row, columns, "LastUpdated", "Last Updated"))
                };

                result.Add(item);
            }

            return (result, skipped);
        }

        private static string GetFirstNonEmpty(IReadOnlyList<object> row, IReadOnlyDictionary<string, int> columns, params string[] possibleNames)
        {
            foreach (var name in possibleNames)
            {
                var value = GoogleSheetHelper.GetValue(row, columns, name);

                if (string.IsNullOrWhiteSpace(value))
                    continue;

                // Skip Google Sheet error values
                if (GoogleSheetHelper.IsGoogleError(value))
                    continue;

                return value;
            }

            return string.Empty;
        }
    }
}