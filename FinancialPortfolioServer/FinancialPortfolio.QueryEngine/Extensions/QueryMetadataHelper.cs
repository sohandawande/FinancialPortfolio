using FinancialPortfolio.QueryEngine.Metadata;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq.Expressions;
using System.Reflection;

namespace FinancialPortfolio.QueryEngine.Extensions
{
    public static class QueryMetadataHelper
    {
        public static bool HasFlag(IModel model, Type clrType, string propertyPath, string annotationKey)
        {
            var resolved = ResolvePath(model, clrType, propertyPath);
            if (resolved is null)
                return false;

            var prop = FindProperty(model, clrType, resolved);
            return prop?.FindAnnotation(annotationKey)?.Value is true;
        }

        public static bool IsSearchable(IModel model, Type clrType, string path)
            => HasFlag(model, clrType, path, QueryEngineMetadata.Searchable);

        public static bool IsFilterable(IModel model, Type clrType, string path)
            => HasFlag(model, clrType, path, QueryEngineMetadata.Filterable);

        public static bool IsSortable(IModel model, Type clrType, string path)
            => HasFlag(model, clrType, path, QueryEngineMetadata.Sortable);

        public static IReadOnlyList<IProperty> GetFlaggedProperties(
            IModel model,
            Type clrType,
            string annotationKey)
        {
            var entityType = model.FindEntityType(clrType);
            if (entityType is null)
                return Array.Empty<IProperty>();

            return entityType
                .GetProperties()
                .Where(p => p.FindAnnotation(annotationKey)?.Value is true)
                .ToList();
        }

        /// <summary>
        /// Turns client field into a real model path.
        /// Examples:
        ///   "symbol"       → "Symbol"
        ///   "currentPrice" → "StockDetail.CurrentPrice"
        ///   "StockDetail.CurrentPrice" → same (normalized casing)
        /// </summary>
        public static string? ResolvePath(IModel model, Type clrType, string? clientField)
        {
            if (string.IsNullOrWhiteSpace(clientField))
                return null;

            var input = clientField.Trim();
            var entityType = model.FindEntityType(clrType);
            if (entityType is null)
                return null;

            // Explicit path: "StockDetail.CurrentPrice"
            if (input.Contains('.', StringComparison.Ordinal))
            {
                return ResolveExplicitPath(entityType, input);
            }

            // 1) Property on root entity
            var rootProp = entityType
                .GetProperties()
                .FirstOrDefault(p => p.Name.Equals(input, StringComparison.OrdinalIgnoreCase));

            if (rootProp is not null)
                return rootProp.Name;

            // 2) Property on any navigation target (depth 1)
            //    currentPrice → StockDetail.CurrentPrice
            foreach (var navigation in entityType.GetNavigations())
            {
                var target = navigation.TargetEntityType;
                var nested = target
                    .GetProperties()
                    .FirstOrDefault(p => p.Name.Equals(input, StringComparison.OrdinalIgnoreCase));

                if (nested is not null)
                    return $"{navigation.Name}.{nested.Name}";
            }

            // 3) Optional: depth-2 navigations (rarely needed)
            foreach (var nav1 in entityType.GetNavigations())
            {
                foreach (var nav2 in nav1.TargetEntityType.GetNavigations())
                {
                    var nested = nav2.TargetEntityType
                        .GetProperties()
                        .FirstOrDefault(p => p.Name.Equals(input, StringComparison.OrdinalIgnoreCase));

                    if (nested is not null)
                        return $"{nav1.Name}.{nav2.Name}.{nested.Name}";
                }
            }

            return null;
        }

        private static string? ResolveExplicitPath(IEntityType entityType, string input)
        {
            var segments = input.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var names = new List<string>();
            var current = entityType;

            for (var i = 0; i < segments.Length; i++)
            {
                var segment = segments[i];
                var isLast = i == segments.Length - 1;

                if (isLast)
                {
                    var prop = current
                        .GetProperties()
                        .FirstOrDefault(p => p.Name.Equals(segment, StringComparison.OrdinalIgnoreCase));

                    if (prop is null)
                        return null;

                    names.Add(prop.Name);
                    return string.Join('.', names);
                }

                var navigation = current
                    .GetNavigations()
                    .FirstOrDefault(n => n.Name.Equals(segment, StringComparison.OrdinalIgnoreCase));

                if (navigation is null)
                    return null;

                names.Add(navigation.Name);
                current = navigation.TargetEntityType;
            }

            return null;
        }

        public static IProperty? FindProperty(IModel model, Type clrType, string propertyPath)
        {
            var resolved = ResolvePath(model, clrType, propertyPath);
            if (resolved is null)
                return null;

            var segments = resolved.Split('.', StringSplitOptions.RemoveEmptyEntries);
            var entityType = model.FindEntityType(clrType);
            if (entityType is null)
                return null;

            for (var i = 0; i < segments.Length; i++)
            {
                var name = segments[i];
                var isLast = i == segments.Length - 1;

                if (isLast)
                {
                    return entityType
                        .GetProperties()
                        .FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                }

                var navigation = entityType
                    .GetNavigations()
                    .FirstOrDefault(n => n.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

                if (navigation is null)
                    return null;

                entityType = navigation.TargetEntityType;
            }

            return null;
        }

        public static Expression PropertyExpression(Expression parameter, string propertyPath)
        {
            if (string.IsNullOrWhiteSpace(propertyPath))
                throw new ArgumentException("Property path is required.", nameof(propertyPath));

            Expression current = parameter;

            foreach (var segment in propertyPath.Split(
                         '.',
                         StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var prop = current.Type.GetProperty(
                               segment,
                               BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)
                           ?? throw new InvalidOperationException(
                               $"Property '{segment}' not found on '{current.Type.Name}' (path '{propertyPath}').");

                current = Expression.Property(current, prop);
            }

            return current;
        }
    }
}
