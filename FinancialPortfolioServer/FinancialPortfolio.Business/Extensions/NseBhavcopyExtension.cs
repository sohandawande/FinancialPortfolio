using FinancialPortfolio.Business.Services.MarketData;
using FinancialPortfolio.Models.Model.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FinancialPortfolio.Business.Extensions
{
    public static class NseBhavcopyExtension
    {
        public static IServiceCollection AddNseBhavcopyConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<NseBhavcopySettings>(configuration.GetSection(NseBhavcopySettings.SectionName));

            services.AddHttpClient("NseArchives", client =>
            {
                client.Timeout = TimeSpan.FromMinutes(2);
                client.DefaultRequestHeaders.UserAgent.ParseAdd(
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/126.0.0.0 Safari/537.36");
                client.DefaultRequestHeaders.Referrer = new Uri("https://www.nseindia.com/all-reports");
                client.DefaultRequestHeaders.Accept.ParseAdd("application/zip,text/csv,application/json,*/*");
            }).ConfigurePrimaryHttpMessageHandler(() => new NseArchiveHttpHandler());

            return services;
        }
    }
}
