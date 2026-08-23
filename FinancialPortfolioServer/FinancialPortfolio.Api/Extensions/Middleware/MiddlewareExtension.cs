using FinancialPortfolio.Api.Middleware;

namespace FinancialPortfolio.Api.Extensions.Middleware
{
    public static class MiddlewareExtension
    {
        public static IApplicationBuilder UseGlobalExceptionMiddleware(this IApplicationBuilder app)
        {
            app.UseMiddleware<ExceptionMiddleware>();

            return app;
        }
    }
}
