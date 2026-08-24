using FinancialPortfolio.Business.Abstractions.IWealth;
using FinancialPortfolio.Models.Model.Settings;
using Microsoft.Extensions.Options;

namespace FinancialPortfolio.Api.Background
{
    public sealed class MutualFundNavSyncHostedService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<MutualFundNavSyncHostedService> _logger;
        private readonly MutualFundNavSettings _settings;

        public MutualFundNavSyncHostedService(
            IServiceScopeFactory scopeFactory,
            IOptions<MutualFundNavSettings> options,
            ILogger<MutualFundNavSyncHostedService> logger)
        {
            _scopeFactory = scopeFactory;
            _settings = options.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_settings.Enabled)
            {
                _logger.LogInformation("Mutual fund NAV daily sync is disabled.");
                return;
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                var delay = DelayUntilNextRun();
                _logger.LogInformation("Next MF NAV sync in {Delay} (21:00 IST default).", delay);
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
                var service = scope.ServiceProvider.GetRequiredService<IMutualFundNavService>();
                var result = await service.SyncAllActiveAsync(cancellationToken);
                _logger.LogInformation("MF NAV sync updated={Updated} failed={Failed}", result.Updated, result.Failed);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "MF NAV sync skipped or failed.");
            }
        }

        private TimeSpan DelayUntilNextRun()
        {
            var ist = TimeZoneInfo.FindSystemTimeZoneById(
                TimeZoneInfo.GetSystemTimeZones().Any(z => z.Id == "India Standard Time")
                    ? "India Standard Time"
                    : "Asia/Kolkata");
            var nowIst = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, ist);
            var next = new DateTime(
                nowIst.Year, nowIst.Month, nowIst.Day,
                Math.Clamp(_settings.DailySyncHourIst, 0, 23),
                Math.Clamp(_settings.DailySyncMinuteIst, 0, 59),
                0);
            if (next <= nowIst) next = next.AddDays(1);
            return next - nowIst;
        }
    }
}
