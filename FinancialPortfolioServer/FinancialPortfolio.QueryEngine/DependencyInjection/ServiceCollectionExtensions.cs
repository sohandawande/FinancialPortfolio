using FinancialPortfolio.QueryEngine.Abstractions.IQuery;
using FinancialPortfolio.QueryEngine.Services.Query;
using Microsoft.Extensions.DependencyInjection;

namespace FinancialPortfolio.QueryEngine.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddQueryEngineLayer(this IServiceCollection services)
        {
            services.AddScoped<IQueryService, QueryService>();

            return services;
        }
    }
}
