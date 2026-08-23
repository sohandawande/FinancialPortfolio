using FinancialPortfolio.Models.Model.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FinancialPortfolio.Business.Extensions
{
    public static class GoogleSheetExtension
    {
        public static IServiceCollection AddGoogleSheetConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpClient("GoogleSheet");
            services.Configure<GoogleSheetSettings>(configuration.GetSection(GoogleSheetSettings.SectionName));
            return services;
        }
    }
}
