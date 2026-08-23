using FinancialPortfolio.Business.Mapping.Authentication;
using FinancialPortfolio.Business.Mapping.Portfolio;
using FinancialPortfolio.Business.Mapping.Stock;
using FinancialPortfolio.Business.Mapping.SystemLog;
using Microsoft.Extensions.DependencyInjection;

namespace FinancialPortfolio.Business.Extensions
{
    public static class AutoMapperExtension
    {
        public static IServiceCollection AddAutoMapperConfiguration(this IServiceCollection services)
        {
            //services.AddAutoMapper(typeof(AutoMapperExtension).Assembly);
            services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<AuthenticationMapperProfile>();
                cfg.AddProfile<StockMapperProfile>();
                cfg.AddProfile<SystemLogMapperProfile>();
                cfg.AddProfile<PortfolioMappingProfile>();
            });
            return services;
        }
    }
}
