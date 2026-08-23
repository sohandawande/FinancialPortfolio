using FinancialPortfolio.QueryEngine.Enums;
using FinancialPortfolio.QueryEngine.Models;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq.Expressions;

namespace FinancialPortfolio.QueryEngine.Extensions
{
    public static class FilterExtensions
    {
        public static IQueryable<T> ApplyFilters<T>(
            this IQueryable<T> query,
            List<FilterRequest>? filters,
            IModel model)
        {
            if (filters is null || filters.Count == 0)
                return query;

            foreach (var filter in filters)
            {
                if (string.IsNullOrWhiteSpace(filter.Field) || filter.Value is null)
                    continue;

                // Dynamic: "symbol" → "Symbol", "currentPrice" → "StockDetail.CurrentPrice"
                var resolvedPath = QueryMetadataHelper.ResolvePath(model, typeof(T), filter.Field);
                if (resolvedPath is null)
                    continue;

                if (!QueryMetadataHelper.IsFilterable(model, typeof(T), resolvedPath))
                    continue;

                var parameter = Expression.Parameter(typeof(T), "x");
                var property = QueryMetadataHelper.PropertyExpression(parameter, resolvedPath);
                var propertyType = Nullable.GetUnderlyingType(property.Type) ?? property.Type;

                Expression body = propertyType == typeof(string)
                    ? BuildStringFilter(property, filter)
                    : BuildComparableFilter(property, propertyType, filter);

                var lambda = Expression.Lambda<Func<T, bool>>(body, parameter);
                query = query.Where(lambda);
            }

            return query;
        }

        private static Expression BuildStringFilter(Expression property, FilterRequest filter)
        {
            var value = Expression.Constant(filter.Value ?? string.Empty, typeof(string));
            var notNull = Expression.NotEqual(
                property,
                Expression.Constant(null, typeof(string)));

            Expression comparison = filter.Operator switch
            {
                FilterOperator.Equals =>
                    Expression.Equal(property, value),

                FilterOperator.NotEquals =>
                    Expression.NotEqual(property, value),

                FilterOperator.StartsWith =>
                    Expression.Call(
                        property,
                        typeof(string).GetMethod(nameof(string.StartsWith), new[] { typeof(string) })!,
                        value),

                FilterOperator.EndsWith =>
                    Expression.Call(
                        property,
                        typeof(string).GetMethod(nameof(string.EndsWith), new[] { typeof(string) })!,
                        value),

                // Contains (3) and default
                _ =>
                    Expression.Call(
                        property,
                        typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) })!,
                        value),
            };

            return Expression.AndAlso(notNull, comparison);
        }

        private static Expression BuildComparableFilter(
            Expression property,
            Type nonNullableType,
            FilterRequest filter)
        {
            object converted = Convert.ChangeType(filter.Value, nonNullableType);
            var constant = Expression.Constant(converted, property.Type);

            return filter.Operator switch
            {
                FilterOperator.Equals =>
                    Expression.Equal(property, constant),

                FilterOperator.NotEquals =>
                    Expression.NotEqual(property, constant),

                FilterOperator.GreaterThan =>
                    Expression.GreaterThan(property, constant),

                FilterOperator.GreaterThanOrEqual =>
                    Expression.GreaterThanOrEqual(property, constant),

                FilterOperator.LessThan =>
                    Expression.LessThan(property, constant),

                FilterOperator.LessThanOrEqual =>
                    Expression.LessThanOrEqual(property, constant),

                _ => Expression.Equal(property, constant),
            };
        }
    }
}