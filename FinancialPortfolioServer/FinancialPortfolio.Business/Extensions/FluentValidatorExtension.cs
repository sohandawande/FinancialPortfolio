using FinancialPortfolio.Business.Validators.Authentication;
using FinancialPortfolio.Business.Validators.Portfolio;
using FinancialPortfolio.Business.Validators.Stock;
using FinancialPortfolio.Business.Validators.SystemLog;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;


namespace FinancialPortfolio.Business.Extensions
{
    public static class FluentValidatorExtension
    {
        public static IServiceCollection AddFluentValidatorConfiguration(this IServiceCollection services)
        {
            services.AddValidatorsFromAssemblyContaining<RefreshTokenRequestValidator>();
            services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();
            services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();
            services.AddValidatorsFromAssemblyContaining<StockCreateRequestValidator>();
            services.AddValidatorsFromAssemblyContaining<StockUpdateRequestValidator>();
            services.AddValidatorsFromAssemblyContaining<ClientLogRequestValidator>();
            services.AddValidatorsFromAssemblyContaining<BuyStockRequestValidator>();
            services.AddValidatorsFromAssemblyContaining<UpdateHoldRequestValidator>();
            services.AddValidatorsFromAssemblyContaining<SellStockRequestValidator>();
            services.AddValidatorsFromAssemblyContaining<UpdateSoldRequestValidator>();
            services.AddValidatorsFromAssemblyContaining<CreatePortfolioRequestValidator>();
            services.AddValidatorsFromAssemblyContaining<UpdatePortfolioRequestValidator>();
            return services;
        }
    }
}
