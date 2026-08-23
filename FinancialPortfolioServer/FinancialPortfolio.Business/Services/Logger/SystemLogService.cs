using AutoMapper;
using FinancialPortfolio.Business.Abstractions.ILogger;
using FinancialPortfolio.Business.Abstractions.IValidation;
using FinancialPortfolio.Business.Common.Helpers;
using FinancialPortfolio.Business.Common.Logging;
using FinancialPortfolio.Data.Context;
using FinancialPortfolio.Data.Entities.SystemLog;
using FinancialPortfolio.Models.Abstractions.ICurrentUser;
using FinancialPortfolio.Models.Common.Enums;
using FinancialPortfolio.Models.Common.Exceptions;
using FinancialPortfolio.Models.Common.Response;
using FinancialPortfolio.Models.Common.Utilities;
using FinancialPortfolio.Models.Model.Request.SystemLog;
using FinancialPortfolio.Models.Model.Response.SystemLog;
using FinancialPortfolio.QueryEngine.Abstractions.IQuery;
using FinancialPortfolio.QueryEngine.Models;
using Microsoft.EntityFrameworkCore;


namespace FinancialPortfolio.Business.Services.Logger
{
    public sealed class SystemLogService : ISystemLogService
    {
        private readonly ApplicationDbContext _context;
        private readonly IQueryService _queryService;
        private readonly IMapper _mapper;
        private readonly IApplicationLoggerService _logger;
        private readonly IValidationService _validation;

        public SystemLogService(ApplicationDbContext context, IQueryService queryService, IMapper mapper, IApplicationLoggerService logger, IValidationService validation, ICurrentUserService currentUserService)
        {
            _context = Guard.AgainstNull(context, nameof(context));
            _queryService = Guard.AgainstNull(queryService, nameof(queryService));
            _mapper = Guard.AgainstNull(mapper, nameof(mapper));
            _logger = Guard.AgainstNull(logger, nameof(logger));
            _validation = Guard.AgainstNull(validation, nameof(validation));
        }

        public async Task<ApiResponse<PagedResponse<SystemLogResponse>>> GetAllAsync(QueryRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                await _validation.ValidateAsync(request, cancellationToken);
                IQueryable<SystemLogEntity> query = _context.SystemLogs.Include(x => x.SystemLogDetail).AsNoTracking();

                var result = await _queryService.ExecuteAsync(query, request, _context.Model);

                var pagedResponse = new PagedResponse<SystemLogResponse>
                {
                    Data = _mapper.Map<List<SystemLogResponse>>(result.Data),
                    TotalRecords = result.TotalRecords,
                    TotalPages = result.TotalPages,
                    PageNumber = result.PageNumber,
                    PageSize = result.PageSize
                };
                await _logger.LogInformationAsync(ApplicationLevelType.Business, LogSourceHelper.Current(), "Logs retrieved successfully.", cancellationToken);
                return ResponseFactory.Success(pagedResponse, "Succcess");
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ApplicationLevelType.Business, LogSourceHelper.Current(), ex.Message, ex, cancellationToken);
                throw;
            }
        }

        public async Task<ApiResponse<SystemLogResponse>> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        {
            try
            {
                var log = await _context.SystemLogs.AsNoTracking().Include(x => x.SystemLogDetail).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);


                if (log is null)
                {
                    throw new NotFoundException("Log not found.");
                }

                var response = _mapper.Map<SystemLogResponse>(log);
                await _logger.LogInformationAsync(ApplicationLevelType.Business, LogSourceHelper.Current(), "Log Get Success.", cancellationToken);
                return ResponseFactory.Success(response, "Succeess");
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ApplicationLevelType.Business, LogSourceHelper.Current(), ex.Message, ex, cancellationToken);
                throw;
            }
        }

        public async Task<ApiResponse<bool>> CreateClientLogAsync(ClientLogRequest request, CancellationToken cancellationToken = default)
        {
            await _validation.ValidateAsync(request, cancellationToken);
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var log = new SystemLogEntity
                {
                    LogLevel = request.Level,
                    ApplicationLevel = ApplicationLevelNames.FrontendClient, // "FinancialPortfolio.Client"
                    Category = string.IsNullOrWhiteSpace(request.Category) ? "Angular" : request.Category.Trim(),
                    Method = string.IsNullOrWhiteSpace(request.Method) ? string.Empty : request.Method.Trim(),
                    Message = string.IsNullOrEmpty(request.Message) ? null : request.Message.Trim(),
                    RequestPath = request.PageUrl,
                    MachineName = string.IsNullOrWhiteSpace(request.UserAgent) ? "Browser" : request.UserAgent.Trim().Length > 100 ? request.UserAgent.Trim()[..100] : request.UserAgent.Trim(),
                    UserId = null,
                    IdentityUserId = null,
                    SystemLogDetail = new SystemLogDetailEntity
                    {
                        Exception = request.Message,
                        StackTrace = request.StackTrace
                    }
                };

                _context.SystemLogs.Add(log);
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                await _logger.LogInformationAsync(ApplicationLevelType.Business, LogSourceHelper.Current(), "Client log saved successfully.", cancellationToken);
                return ResponseFactory.Success(true, "Client log saved successfully.");
            }
            catch (Exception ex)
            {
                {
                    await transaction.RollbackAsync(cancellationToken);
                    await _logger.LogErrorAsync(ApplicationLevelType.Business, LogSourceHelper.Current(), ex.Message, ex, cancellationToken);
                    throw;
                }
            }
        }
    }
}