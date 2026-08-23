using FinancialPortfolio.Business.Abstractions.ILogger;
using FinancialPortfolio.Business.Common.Logging;
using FinancialPortfolio.Data.Context;
using FinancialPortfolio.Data.Entities.SystemLog;
using FinancialPortfolio.Models.Abstractions.ICurrentUser;
using FinancialPortfolio.Models.Common.Enums;
using FinancialPortfolio.Models.Common.Utilities;
using Microsoft.AspNetCore.Http;
using System.Reflection;

namespace FinancialPortfolio.Business.Services.Logger
{
    public sealed class ApplicationLoggerService : IApplicationLoggerService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly TimeProvider _timeProvider;
        private static readonly Assembly CurrentServiceAssembly = typeof(ApplicationLoggerService).Assembly;

        public ApplicationLoggerService(ApplicationDbContext context, ICurrentUserService currentUserService, IHttpContextAccessor httpContextAccessor, TimeProvider timeProvider)
        {
            _context = Guard.AgainstNull(context, nameof(context));
            _currentUserService = Guard.AgainstNull(currentUserService, nameof(currentUserService));
            _httpContextAccessor = Guard.AgainstNull(httpContextAccessor, nameof(httpContextAccessor));
            _timeProvider = Guard.AgainstNull(timeProvider, nameof(timeProvider));
        }

        public Task LogInformationAsync(ApplicationLevelType applicationLevel, LogSource source, string message, CancellationToken cancellationToken = default)
        {
            return LogAsync(LogLevelType.Information, applicationLevel, source, message, null, cancellationToken);
        }

        public Task LogWarningAsync(ApplicationLevelType applicationLevel, LogSource source, string message, CancellationToken cancellationToken = default)
        {
            return LogAsync(LogLevelType.Warning, applicationLevel, source, message, null, cancellationToken);
        }


        public Task LogErrorAsync(ApplicationLevelType applicationLevel, LogSource source, string message, Exception exception, CancellationToken cancellationToken = default)
        {
            return LogAsync(LogLevelType.Error, applicationLevel, source, message, exception, cancellationToken);
        }

        public Task LogCriticalAsync(ApplicationLevelType applicationLevel, LogSource source, string message, Exception exception, CancellationToken cancellationToken = default)
        {
            return LogAsync(LogLevelType.Critical, applicationLevel, source, message, exception, cancellationToken);
        }

        public async Task LogAsync(LogLevelType level, ApplicationLevelType applicationLevel, LogSource source, string message, Exception? exception = null, CancellationToken cancellationToken = default)
        {
            var http = _httpContextAccessor.HttpContext;
            try
            {
                var entity = new SystemLogEntity
                {
                    LogLevel = level,
                    ApplicationLevel = ApplicationLevelNames.ToStorage(applicationLevel),
                    Category = source.Category,
                    Method = source.Method,
                    Message = message?.Trim() ?? string.Empty,
                    UserId = _currentUserService.IsAuthenticated ? _currentUserService.UserId : null,
                    IdentityUserId = _currentUserService.IsAuthenticated ? _currentUserService.IdentityUserId : null,
                    RequestPath = http?.Request.Path.Value,
                    IpAddress = http?.Connection.RemoteIpAddress?.ToString(),
                    MachineName = Environment.MachineName,
                    SystemLogDetail = new SystemLogDetailEntity
                    {
                        Exception = exception?.Message,
                        StackTrace = exception?.ToString()
                    }
                };

                _context.SystemLogs.Add(entity);
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                {
                    throw;
                }
            }
        }
    }
}
