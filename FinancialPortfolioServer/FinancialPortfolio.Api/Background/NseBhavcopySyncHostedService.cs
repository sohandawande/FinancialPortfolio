using FinancialPortfolio.Business.Abstractions.IMarketData;
using FinancialPortfolio.Models.Model.Request.MarketData;
using FinancialPortfolio.Models.Model.Settings;
using Microsoft.Extensions.Options;

namespace FinancialPortfolio.Api.Background
{
    public sealed class NseBhavcopySyncHostedService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<NseBhavcopySyncHostedService> _logger;
        private readonly NseBhavcopySettings _settings;

        public NseBhavcopySyncHostedService(
            IServiceScopeFactory scopeFactory,
            IOptions<NseBhavcopySettings> options,
            ILogger<NseBhavcopySyncHostedService> logger)
        {
            _scopeFactory = scopeFactory;
            _settings = options.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_settings.Enabled)
            {
                _logger.LogInformation("NSE bhavcopy daily sync is disabled.");
                return;
            }

            // Catch up once after startup (covers missed evening runs).
            await RunOnceSafe(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                var delay = DelayUntilNextRun();
                _logger.LogInformation("Next NSE bhavcopy sync in {Delay}.", delay);

                try
                {
                    await Task.Delay(delay, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                await RunOnceSafe(stoppingToken);
            }
        }

        private async Task RunOnceSafe(CancellationToken cancellationToken)
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var service = scope.ServiceProvider.GetRequiredService<INseBhavcopyService>();
                var result = await service.SyncAsync(new MarketDataSyncRequest(), cancellationToken);
                _logger.LogInformation(
                    "NSE sync finished. Date={Date} Inserted={Inserted} Updated={Updated}",
                    result.Data?.TradeDate,
                    result.Data?.InsertedRecords,
                    result.Data?.UpdatedRecords);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "NSE bhavcopy sync skipped or failed.");
            }
        }

        private TimeSpan DelayUntilNextRun()
        {
            var nowIst = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Ist());
            var next = new DateTime(
                nowIst.Year,
                nowIst.Month,
                nowIst.Day,
                Math.Clamp(_settings.DailySyncHourIst, 0, 23),
                Math.Clamp(_settings.DailySyncMinuteIst, 0, 59),
                0,
                DateTimeKind.Unspecified);

            if (nowIst >= next)
                next = next.AddDays(1);

            var nextUtc = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(next, DateTimeKind.Unspecified), Ist());
            var delay = nextUtc - DateTime.UtcNow;
            return delay < TimeSpan.FromMinutes(1) ? TimeSpan.FromMinutes(1) : delay;
        }

        private static TimeZoneInfo Ist() => TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");
    }
}
