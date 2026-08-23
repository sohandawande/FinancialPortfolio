using FinancialPortfolio.QueryEngine.Metadata;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq.Expressions;

namespace FinancialPortfolio.QueryEngine.Extensions
{
    public static class SearchExtensions
    {
        public static IQueryable<T> ApplySearch<T>(this IQueryable<T> query, string? search, IModel model)
        {
            if (string.IsNullOrWhiteSpace(search))
                return query;

            var searchable = QueryMetadataHelper
                .GetFlaggedProperties(model, typeof(T), QueryEngineMetadata.Searchable)
                .Where(p => p.ClrType == typeof(string))
                .ToList();

            if (searchable.Count == 0)
                return query;

            var parameter = Expression.Parameter(typeof(T), "x");
            Expression? combined = null;

            var containsMethod = typeof(string).GetMethod(
                nameof(string.Contains), new[] { typeof(string) })!;

            var term = Expression.Constant(search.Trim());

            foreach (var prop in searchable)
            {
                var access = Expression.Property(parameter, prop.Name);

                var notNull = Expression.NotEqual(access, Expression.Constant(null, typeof(string)));

                var contains = Expression.Call(access, containsMethod, term);
                var clause = Expression.AndAlso(notNull, contains);

                combined = combined is null ? clause : Expression.OrElse(combined, clause);
            }

            var lambda = Expression.Lambda<Func<T, bool>>(combined!, parameter);
            return query.Where(lambda);
        }
    }
}
