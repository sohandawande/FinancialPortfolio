using AutoMapper;
using FinancialPortfolio.Business.Abstractions.ILogger;
using FinancialPortfolio.Business.Abstractions.IEtf;
using FinancialPortfolio.Business.Abstractions.IValidation;
using FinancialPortfolio.Business.Common.Helpers;
using FinancialPortfolio.Data.Context;
using FinancialPortfolio.Data.Entities.Etf;
using FinancialPortfolio.Models.Common.Enums;
using FinancialPortfolio.Models.Common.Exceptions;
using FinancialPortfolio.Models.Common.Response;
using FinancialPortfolio.Models.Common.Utilities;
using FinancialPortfolio.Models.Model.Request.Etf;
using FinancialPortfolio.Models.Model.Response.Etf;
using FinancialPortfolio.QueryEngine.Abstractions.IQuery;
using FinancialPortfolio.QueryEngine.Models;
using Microsoft.EntityFrameworkCore;

namespace FinancialPortfolio.Business.Services.Etf
{
    public sealed class EtfService : IEtfService
    {
        private readonly ApplicationDbContext _context;
        private readonly IQueryService _queryService;
        private readonly IMapper _mapper;
        private readonly IApplicationLoggerService _logger;
        private readonly IValidationService _validation;

        public EtfService(ApplicationDbContext context, IQueryService queryService, IMapper mapper, IApplicationLoggerService logger, IValidationService validation)
        {
            _context = Guard.AgainstNull(context, nameof(context));
            _queryService = Guard.AgainstNull(queryService, nameof(queryService));
            _mapper = Guard.AgainstNull(mapper, nameof(mapper));
            _logger = Guard.AgainstNull(logger, nameof(logger));
            _validation = Guard.AgainstNull(validation, nameof(validation));
        }

        public async Task<ApiResponse<PagedResponse<EtfsResponse>>> SearchAsync(QueryRequest request, CancellationToken cancellationToken)
        {
            Guard.AgainstNull(request, nameof(request));
            try
            {
                await _validation.ValidateAsync(request, cancellationToken);

                IQueryable<EtfEntity> query = _context.Etfs
                    .Include(x => x.EtfDetail)
                    .AsNoTracking();

                var result = await _queryService.ExecuteAsync(query, request, _context.Model);

                var pagedResponse = new PagedResponse<EtfsResponse>
                {
                    Data = _mapper.Map<List<EtfsResponse>>(result.Data),
                    TotalRecords = result.TotalRecords,
                    TotalPages = result.TotalPages,
                    PageNumber = result.PageNumber,
                    PageSize = result.PageSize
                };

                await _logger.LogInformationAsync(ApplicationLevelType.Business, LogSourceHelper.Current(), "ETF Search Success.", cancellationToken);

                return ResponseFactory.Success(pagedResponse, "Success");
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ApplicationLevelType.Business, LogSourceHelper.Current(), ex.Message, ex, cancellationToken);
                throw;
            }
        }

        public async Task<ApiResponse<EtfsResponse>> GetByIdAsync(long id, CancellationToken cancellationToken)
        {
            try
            {
                var stock = await _context.Etfs.AsNoTracking().Include(x => x.EtfDetail).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

                if (stock is null)
                {
                    throw new NotFoundException("ValidationMessageConstants.EtfNotFound");
                }

                var response = _mapper.Map<EtfsResponse>(stock);
                await _logger.LogInformationAsync(ApplicationLevelType.Business, LogSourceHelper.Current(), "ETF Get Success.", cancellationToken);
                return ResponseFactory.Success(response, "Succeess");
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ApplicationLevelType.Business, LogSourceHelper.Current(), ex.Message, ex, cancellationToken);
                throw;
            }
        }

        public async Task<ApiResponse<EtfsResponse>> CreateAsync(EtfCreateRequest request, CancellationToken cancellationToken)
        {
            Guard.AgainstNull(request, nameof(request));
            await _validation.ValidateAsync(request, cancellationToken);
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var stock = _mapper.Map<EtfEntity>(request);

                await _context.Etfs.AddAsync(stock, cancellationToken);

                await _context.SaveChangesAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                var response = _mapper.Map<EtfsResponse>(stock);

                await _logger.LogInformationAsync(ApplicationLevelType.Business, LogSourceHelper.Current(), "ETF Created Successfully.", cancellationToken);
                return ResponseFactory.Success(response, "ETF Created Successfully");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                await _logger.LogErrorAsync(ApplicationLevelType.Business, LogSourceHelper.Current(), ex.Message, ex, cancellationToken);
                throw;
            }
        }

        public async Task<ApiResponse<EtfsResponse>> UpdateAsync(long id, EtfUpdateRequest request, CancellationToken cancellationToken)
        {
            Guard.AgainstNull(request, nameof(request));
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var stock = await _context.Etfs.Include(x => x.EtfDetail).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

                if (stock == null)
                {
                    throw new NotFoundException("ValidationMessageConstants.EtfNotFound");
                }


                _mapper.Map(request, stock);

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                var response = _mapper.Map<EtfsResponse>(stock);
                await _logger.LogInformationAsync(ApplicationLevelType.Business, LogSourceHelper.Current(), "ETF Updated Successfully.", cancellationToken);
                return ResponseFactory.Success(response, "Update Successfully");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                await _logger.LogErrorAsync(ApplicationLevelType.Business, LogSourceHelper.Current(), ex.Message, ex, cancellationToken);
                throw;
            }
        }

        public async Task<ApiResponse<EtfsResponse>> DeleteAsync(long id, CancellationToken cancellationToken)
        {
            try
            {
                var stock = await _context.Etfs.Include(x => x.EtfDetail).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

                if (stock == null)
                {
                    throw new NotFoundException("ValidationMessageConstants.EtfNotFound");
                }

                _context.Etfs.Remove(stock);

                await _context.SaveChangesAsync(cancellationToken);

                var response = _mapper.Map<EtfsResponse>(stock);
                await _logger.LogInformationAsync(ApplicationLevelType.Business, LogSourceHelper.Current(), "ETF Deleted Successfully.", cancellationToken);
                return ResponseFactory.Success(response, "Deleted Successfully");
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ApplicationLevelType.Business, LogSourceHelper.Current(), ex.Message, ex, cancellationToken);
                throw;
            }
        }
    }
}
