using FinancialPortfolio.QueryEngine.Enums;
using FinancialPortfolio.QueryEngine.Models;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq.Expressions;

namespace FinancialPortfolio.QueryEngine.Extensions
{
    public static class SortExtensions
    {
        public static IQueryable<T> ApplySorting<T>(
            this IQueryable<T> query,
            List<SortRequest>? sorts,
            IModel model)
        {
            if (sorts is null || sorts.Count == 0)
                return query;

            IOrderedQueryable<T>? ordered = null;

            foreach (var sort in sorts)
            {
                if (string.IsNullOrWhiteSpace(sort.Field))
                    continue;

                // Dynamic: "symbol" → "Symbol", "currentPrice" → "StockDetail.CurrentPrice"
                var resolvedPath = QueryMetadataHelper.ResolvePath(model, typeof(T), sort.Field);
                if (resolvedPath is null)
                    continue;

                if (!QueryMetadataHelper.IsSortable(model, typeof(T), resolvedPath))
                    continue;

                var parameter = Expression.Parameter(typeof(T), "x");
                var property = QueryMetadataHelper.PropertyExpression(parameter, resolvedPath);
                var lambda = Expression.Lambda(property, parameter);

                var methodName = ordered is null
                    ? (sort.Direction == SortDirection.Desc ? "OrderByDescending" : "OrderBy")
                    : (sort.Direction == SortDirection.Desc ? "ThenByDescending" : "ThenBy");

                var source = ordered is null ? (IQueryable<T>)query : ordered;

                var call = Expression.Call(
                    typeof(Queryable),
                    methodName,
                    new[] { typeof(T), property.Type },
                    source.Expression,
                    Expression.Quote(lambda));

                ordered = (IOrderedQueryable<T>)query.Provider.CreateQuery<T>(call);
            }

            return ordered ?? query;
        }
    }
}