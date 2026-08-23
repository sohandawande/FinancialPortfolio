using FinancialPortfolio.QueryEngine.Abstractions.IQuery;
using FinancialPortfolio.QueryEngine.Exceptions;
using FinancialPortfolio.QueryEngine.Extensions;
using FinancialPortfolio.QueryEngine.Models;
using FinancialPortfolio.QueryEngine.Validators;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace FinancialPortfolio.QueryEngine.Services.Query
{
    public sealed class QueryService : IQueryService
    {
        public async Task<PagedResponse<T>> ExecuteAsync<T>(IQueryable<T> query, QueryRequest? request, IModel model) where T : class
        {
            request ??= new QueryRequest();

            var validation = QueryValidator.Validate<T>(request, model);
            if (!validation.IsValid)
                throw new QueryValidationException(validation.Errors);

            query = query.ApplyQuery(request, model);

            var totalRecords = await query.CountAsync();
            var data = await query.ApplyPaging(request.PageNumber, request.PageSize).ToListAsync();

            return new PagedResponse<T>
            {
                Data = data,
                TotalRecords = totalRecords,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling(totalRecords / (double)request.PageSize),
            };
        }
    }
}
