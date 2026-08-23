using FinancialPortfolio.Business.Common.Logging;
using FinancialPortfolio.Models.Common.Enums;

namespace FinancialPortfolio.Business.Abstractions.ILogger
{
    public interface IApplicationLoggerService
    {
        Task LogInformationAsync(ApplicationLevelType applicationLevel, LogSource source, string message, CancellationToken cancellationToken = default);
        Task LogWarningAsync(ApplicationLevelType applicationLevel, LogSource source, string message, CancellationToken cancellationToken = default);
        Task LogErrorAsync(ApplicationLevelType applicationLevel, LogSource source, string message, Exception exception, CancellationToken cancellationToken = default);
        Task LogCriticalAsync(ApplicationLevelType applicationLevel, LogSource source, string message, Exception exception, CancellationToken cancellationToken = default);
        Task LogAsync(LogLevelType level, ApplicationLevelType applicationLevel, LogSource source, string message, Exception? exception = null, CancellationToken cancellationToken = default);
    }
}
