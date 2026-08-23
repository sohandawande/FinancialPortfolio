using FinancialPortfolio.QueryEngine.Models;
using Microsoft.EntityFrameworkCore.Metadata;

namespace FinancialPortfolio.QueryEngine.Extensions
{
    public static class QueryExtensions
    {
        public static IQueryable<T> ApplyQuery<T>(this IQueryable<T> query, QueryRequest? request, IModel model)
        {
            request ??= new QueryRequest();

            query = query.ApplySearch(request.GlobalSearch, model);
            query = query.ApplyFilters(request.Filters, model);
            query = query.ApplySorting(request.Sorts, model);

            return query;
        }
    }
}
