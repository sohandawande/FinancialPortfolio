using FinancialPortfolio.Models.Common.Exceptions;
using FinancialPortfolio.Models.Common.Response;

namespace FinancialPortfolio.Api.Middleware
{
    public sealed class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception exception)
            {
                await HandleExceptionAsync(context, exception);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            int statusCode;
            string message;
            IEnumerable<string>? errors = null;

            switch (exception)
            {
                case ValidationException validationException:
                    statusCode = StatusCodes.Status400BadRequest;
                    errors = validationException.Errors?.Where(e => !string.IsNullOrWhiteSpace(e)).ToList();
                    message = errors?.FirstOrDefault()
                        ?? (string.IsNullOrWhiteSpace(validationException.Message)
                            || validationException.Message == "One or more validation failures occurred."
                            ? "Please fix the highlighted fields."
                            : validationException.Message);
                    break;

                case NotFoundException:
                    statusCode = StatusCodes.Status404NotFound;
                    message = exception.Message;
                    break;

                case ConflictException:
                    statusCode = StatusCodes.Status409Conflict;
                    message = exception.Message;
                    break;

                case UnauthorizedException:
                    statusCode = StatusCodes.Status401Unauthorized;
                    message = exception.Message;
                    break;

                case ForbiddenException:
                    statusCode = StatusCodes.Status403Forbidden;
                    message = exception.Message;
                    break;

                default:
                    statusCode = StatusCodes.Status500InternalServerError;
                    message = "An unexpected error occurred.";
                    break;
            }

            context.Response.StatusCode = statusCode;

            var response = new ApiResponse<object>
            {
                Success = false,
                Message = message,
                Errors = errors?.ToList() ?? new List<string> { message }
            };

            await context.Response.WriteAsJsonAsync(response);
        }
    }
}
