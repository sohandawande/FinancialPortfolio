using FinancialPortfolio.QueryEngine.Extensions;
using FinancialPortfolio.QueryEngine.Models;
using Microsoft.EntityFrameworkCore.Metadata;

namespace FinancialPortfolio.QueryEngine.Validators
{
    public static class QueryValidator
    {
        private const int MaxPageSize = 100;

        public static ValidationResult Validate<T>(QueryRequest? request, IModel model)
        {
            request ??= new QueryRequest();
            var result = new ValidationResult();

            ValidatePagination(request, result);
            ValidateFilters<T>(request, result, model);
            ValidateSorts<T>(request, result, model);

            return result;
        }

        private static void ValidatePagination(QueryRequest request, ValidationResult result)
        {
            if (request.PageNumber <= 0)
                result.Errors.Add("PageNumber must be greater than 0.");

            if (request.PageSize <= 0)
                result.Errors.Add("PageSize must be greater than 0.");

            if (request.PageSize > MaxPageSize)
                result.Errors.Add($"PageSize cannot exceed {MaxPageSize}.");
        }

        private static void ValidateFilters<T>(QueryRequest request, ValidationResult result, IModel model)
        {
            if (request.Filters is null)
                return;

            foreach (var filter in request.Filters)
            {
                if (string.IsNullOrWhiteSpace(filter.Field))
                {
                    result.Errors.Add("Filter field is required.");
                    continue;
                }

                var prop = QueryMetadataHelper.FindProperty(model, typeof(T), filter.Field);
                if (prop is null)
                {
                    result.Errors.Add($"Filter field '{filter.Field}' does not exist.");
                    continue;
                }

                if (!QueryMetadataHelper.IsFilterable(model, typeof(T), filter.Field))
                {
                    result.Errors.Add($"Field '{filter.Field}' is not filterable.");
                }
            }
        }

        private static void ValidateSorts<T>(QueryRequest request, ValidationResult result, IModel model)
        {
            if (request.Sorts is null)
                return;

            foreach (var sort in request.Sorts)
            {
                if (string.IsNullOrWhiteSpace(sort.Field))
                    continue;

                var prop = QueryMetadataHelper.FindProperty(model, typeof(T), sort.Field);
                if (prop is null)
                {
                    result.Errors.Add($"Sort field '{sort.Field}' does not exist.");
                    continue;
                }

                if (!QueryMetadataHelper.IsSortable(model, typeof(T), sort.Field))
                {
                    result.Errors.Add($"Field '{sort.Field}' is not sortable.");
                }
            }
        }
    }
}
