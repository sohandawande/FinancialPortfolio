using FinancialPortfolio.QueryEngine.Models;
using Microsoft.EntityFrameworkCore.Metadata;

namespace FinancialPortfolio.QueryEngine.Abstractions.IQuery
{
    public interface IQueryService
    {
        Task<PagedResponse<T>> ExecuteAsync<T>(IQueryable<T> query, QueryRequest request, IModel model) where T : class;
    }
}
