using FinancialPortfolio.Business.Abstractions.ILogger;
using FinancialPortfolio.Business.Abstractions.ILogo;
using FinancialPortfolio.Business.Common.Helpers;
using FinancialPortfolio.Models.Common.Enums;
using FinancialPortfolio.Models.Common.Utilities;
using FinancialPortfolio.Models.Model.Settings;
using Microsoft.Extensions.Options;

namespace FinancialPortfolio.Business.Services.Logo
{
    public sealed class LogoService : ILogoService
    {
        private readonly LogoSettings _settings;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IApplicationLoggerService _logger;

        public LogoService(
            IOptions<LogoSettings> options,
            IHttpClientFactory httpClientFactory,
            IApplicationLoggerService logger)
        {
            _settings = Guard.AgainstNull(options.Value, nameof(options));
            _httpClientFactory = Guard.AgainstNull(httpClientFactory, nameof(httpClientFactory));
            _logger = Guard.AgainstNull(logger, nameof(logger));
        }

        public async Task<string?> EnsureLogoAsync(
            string symbol,
            CancellationToken cancellationToken = default)
        {
            symbol = (symbol ?? string.Empty).Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(symbol))
                return null;

            if (string.IsNullOrWhiteSpace(_settings.PublishableToken))
                return null;

            var fileName = $"{symbol}.png";
            var folder = Path.GetFullPath(
                Path.Combine(Directory.GetCurrentDirectory(), _settings.StorageFolder));
            var physicalPath = Path.Combine(folder, fileName);
            var publicPath = $"{_settings.PublicPathPrefix.TrimEnd('/')}/{fileName}";

            if (System.IO.File.Exists(physicalPath))
                return publicPath;

            var ticker = $"{symbol}{_settings.ExchangeSuffix}";
            var url =
                $"https://img.logo.dev/ticker/{Uri.EscapeDataString(ticker)}" +
                $"?token={Uri.EscapeDataString(_settings.PublishableToken)}" +
                $"&size=128&format=png";

            try
            {
                var client = _httpClientFactory.CreateClient(nameof(LogoService));
                using var response = await client.GetAsync(url, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    await _logger.LogWarningAsync(
                        ApplicationLevelType.Business,
                        LogSourceHelper.Current(),
                        $"Logo download failed for {symbol}: {(int)response.StatusCode}",
                        cancellationToken);
                    return null;
                }

                var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
                if (bytes.Length < 200)
                    return null;

                Directory.CreateDirectory(folder);
                await System.IO.File.WriteAllBytesAsync(physicalPath, bytes, cancellationToken);

                await _logger.LogInformationAsync(
                    ApplicationLevelType.Business,
                    LogSourceHelper.Current(),
                    $"Logo saved for {symbol} → {publicPath}",
                    cancellationToken);

                return publicPath;
            }
            catch (Exception ex)
            {
                await _logger.LogWarningAsync(
                    ApplicationLevelType.Business,
                    LogSourceHelper.Current(),
                    $"Logo error for {symbol}: {ex.Message}",
                    cancellationToken);
                return null;
            }
        }
    }
}
