using AutoMapper;
using FinancialPortfolio.Business.Abstractions.ILogger;
using FinancialPortfolio.Business.Abstractions.IStock;
using FinancialPortfolio.Business.Abstractions.IValidation;
using FinancialPortfolio.Business.Common.Helpers;
using FinancialPortfolio.Data.Context;
using FinancialPortfolio.Data.Entities.Stock;
using FinancialPortfolio.Models.Common.Enums;
using FinancialPortfolio.Models.Common.Exceptions;
using FinancialPortfolio.Models.Common.Response;
using FinancialPortfolio.Models.Common.Utilities;
using FinancialPortfolio.Models.Model.Request.Stock;
using FinancialPortfolio.Models.Model.Response.Stock;
using FinancialPortfolio.QueryEngine.Abstractions.IQuery;
using FinancialPortfolio.QueryEngine.Models;
using Microsoft.EntityFrameworkCore;

namespace FinancialPortfolio.Business.Services.Stock
{
    public sealed class StockService : IStockService
    {
        private readonly ApplicationDbContext _context;
        private readonly IQueryService _queryService;
        private readonly IMapper _mapper;
        private readonly IApplicationLoggerService _logger;
        private readonly IValidationService _validation;

        public StockService(ApplicationDbContext context, IQueryService queryService, IMapper mapper, IApplicationLoggerService logger, IValidationService validation)
        {
            _context = Guard.AgainstNull(context, nameof(context));
            _queryService = Guard.AgainstNull(queryService, nameof(queryService));
            _mapper = Guard.AgainstNull(mapper, nameof(mapper));
            _logger = Guard.AgainstNull(logger, nameof(logger));
            _validation = Guard.AgainstNull(validation, nameof(validation));
        }

        public async Task<ApiResponse<PagedResponse<StocksResponse>>> SearchAsync(QueryRequest request, CancellationToken cancellationToken)
        {
            Guard.AgainstNull(request, nameof(request));
            try
            {
                await _validation.ValidateAsync(request, cancellationToken);

                IQueryable<StockEntity> query = _context.Stocks
                    .Include(x => x.StockDetail)
                    .AsNoTracking();

                var result = await _queryService.ExecuteAsync(query, request, _context.Model);

                var pagedResponse = new PagedResponse<StocksResponse>
                {
                    Data = _mapper.Map<List<StocksResponse>>(result.Data),
                    TotalRecords = result.TotalRecords,
                    TotalPages = result.TotalPages,
                    PageNumber = result.PageNumber,
                    PageSize = result.PageSize
                };

                await _logger.LogInformationAsync(ApplicationLevelType.Business, LogSourceHelper.Current(), "Stock Search Success.", cancellationToken);

                return ResponseFactory.Success(pagedResponse, "Success");
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ApplicationLevelType.Business, LogSourceHelper.Current(), ex.Message, ex, cancellationToken);
                throw;
            }
        }

        public async Task<ApiResponse<StocksResponse>> GetByIdAsync(long id, CancellationToken cancellationToken)
        {
            try
            {
                var stock = await _context.Stocks.AsNoTracking().Include(x => x.StockDetail).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

                if (stock is null)
                {
                    throw new NotFoundException("ValidationMessageConstants.StockNotFound");
                }

                var response = _mapper.Map<StocksResponse>(stock);
                await _logger.LogInformationAsync(ApplicationLevelType.Business, LogSourceHelper.Current(), "Stock Get Success.", cancellationToken);
                return ResponseFactory.Success(response, "Succeess");
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ApplicationLevelType.Business, LogSourceHelper.Current(), ex.Message, ex, cancellationToken);
                throw;
            }
        }

        public async Task<ApiResponse<StocksResponse>> CreateAsync(StockCreateRequest request, CancellationToken cancellationToken)
        {
            Guard.AgainstNull(request, nameof(request));
            await _validation.ValidateAsync(request, cancellationToken);
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var stock = _mapper.Map<StockEntity>(request);

                await _context.Stocks.AddAsync(stock, cancellationToken);

                await _context.SaveChangesAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                var response = _mapper.Map<StocksResponse>(stock);

                await _logger.LogInformationAsync(ApplicationLevelType.Business, LogSourceHelper.Current(), "Stock Created Successfully.", cancellationToken);
                return ResponseFactory.Success(response, "Stock Created Successfully");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                await _logger.LogErrorAsync(ApplicationLevelType.Business, LogSourceHelper.Current(), ex.Message, ex, cancellationToken);
                throw;
            }
        }

        public async Task<ApiResponse<StocksResponse>> UpdateAsync(long id, StockUpdateRequest request, CancellationToken cancellationToken)
        {
            Guard.AgainstNull(request, nameof(request));
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var stock = await _context.Stocks.Include(x => x.StockDetail).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

                if (stock == null)
                {
                    throw new NotFoundException("ValidationMessageConstants.StockNotFound");
                }


                _mapper.Map(request, stock);

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                var response = _mapper.Map<StocksResponse>(stock);
                await _logger.LogInformationAsync(ApplicationLevelType.Business, LogSourceHelper.Current(), "Stock Updated Successfully.", cancellationToken);
                return ResponseFactory.Success(response, "Update Successfully");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                await _logger.LogErrorAsync(ApplicationLevelType.Business, LogSourceHelper.Current(), ex.Message, ex, cancellationToken);
                throw;
            }
        }

        public async Task<ApiResponse<StocksResponse>> DeleteAsync(long id, CancellationToken cancellationToken)
        {
            try
            {
                var stock = await _context.Stocks.Include(x => x.StockDetail).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

                if (stock == null)
                {
                    throw new NotFoundException("ValidationMessageConstants.StockNotFound");
                }

                _context.Stocks.Remove(stock);

                await _context.SaveChangesAsync(cancellationToken);

                var response = _mapper.Map<StocksResponse>(stock);
                await _logger.LogInformationAsync(ApplicationLevelType.Business, LogSourceHelper.Current(), "Stock Deleted Successfully.", cancellationToken);
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
