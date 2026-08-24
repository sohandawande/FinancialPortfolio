using FinancialPortfolio.Business.Abstractions.IAppUser;
using FinancialPortfolio.Business.Abstractions.IAuthentication;
using FinancialPortfolio.Business.Abstractions.IEmail;
using FinancialPortfolio.Business.Abstractions.IGoogleSheet;
using FinancialPortfolio.Business.Abstractions.IJwtToken;
using FinancialPortfolio.Business.Abstractions.ILogger;
using FinancialPortfolio.Business.Abstractions.ILogo;
using FinancialPortfolio.Business.Abstractions.IPortfolio;
using FinancialPortfolio.Business.Abstractions.IStock;
using FinancialPortfolio.Business.Abstractions.IValidation;
using FinancialPortfolio.Business.Abstractions.IWealth;
using FinancialPortfolio.Business.Extensions;
using FinancialPortfolio.Business.Mapping.Resolver;
using FinancialPortfolio.Business.Services.Authentication;
using FinancialPortfolio.Business.Services.CurrentUser;
using FinancialPortfolio.Business.Services.Email;
using FinancialPortfolio.Business.Services.GoogleSheet;
using FinancialPortfolio.Business.Services.JwtToken;
using FinancialPortfolio.Business.Services.Logger;
using FinancialPortfolio.Business.Services.Logo;
using FinancialPortfolio.Business.Services.Portfolio;
using FinancialPortfolio.Business.Services.Stock;
using FinancialPortfolio.Business.Services.User;
using FinancialPortfolio.Business.Services.Validation;
using FinancialPortfolio.Business.Services.Wealth;
using FinancialPortfolio.Models.Abstractions.ICurrentUser;
using FinancialPortfolio.Models.Model.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FinancialPortfolio.Business.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBusinessLayer(this IServiceCollection services, IConfiguration configuration)
        {
            // AutoMapper
            services.AddAutoMapperConfiguration();
            // Fluent Validator
            services.AddFluentValidatorConfiguration();
            services.AddGoogleSheetConfiguration(configuration);
            // Logo settings
            services.Configure<LogoSettings>(configuration.GetSection(LogoSettings.SectionName));
            services.Configure<SeedSettings>(configuration.GetSection(SeedSettings.SectionName));
            services.AddHttpClient(nameof(LogoService));
            services.AddScoped<PublicLogoUrlResolver>();

            // Services
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddScoped<IValidationService, ValidationService>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IApplicationLoggerService, ApplicationLoggerService>();
            services.AddScoped<IGoogleSheetService, GoogleSheetService>();
            services.AddScoped<IGoogleSheetSyncService, GoogleSheetSyncService>();
            services.AddScoped<IStockService, StockService>();
            services.AddScoped<IAppUserService, AppUserService>();
            services.AddScoped<ISystemLogService, SystemLogService>();
            services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));
            services.AddScoped<IEmailTemplateService, EmailTemplateService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<ILogoService, LogoService>();
            services.AddScoped<IPortfolioService, PortfolioService>();
            services.AddScoped<IWealthTrackerService, WealthTrackerService>();
            services.Configure<MutualFundNavSettings>(configuration.GetSection(MutualFundNavSettings.SectionName));
            services.AddHttpClient<MutualFundNavService>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.UserAgent.ParseAdd("FinancialPortfolio/1.0");
            });
            services.AddScoped<IMutualFundNavService>(sp => sp.GetRequiredService<MutualFundNavService>());

            return services;
        }
    }
}
