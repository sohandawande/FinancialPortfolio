namespace FinancialPortfolio.Api.DependencyInjection
{
    using FinancialPortfolio.Api.Configurations.Jwt;
    using FinancialPortfolio.Api.Configurations.Swagger;
    using FinancialPortfolio.Api.Extensions.Identity;
    using FinancialPortfolio.Api.Services.Realtime;
    using FinancialPortfolio.Business.Abstractions.INotification;
    using FinancialPortfolio.Data.Interceptors;

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApiServiceLayer(this IServiceCollection services, IConfiguration configuration)
        {
            //services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
            //services.AddGoogleSheetConfiguration(configuration);
            services.AddSwaggerConfiguration();
            services.AddIdentityServices();
            services.AddJwtConfiguration(configuration);
            services.AddHttpContextAccessor();

            services.AddSingleton<TimeProvider>(TimeProvider.System);
            services.AddScoped<AuditSaveChangesInterceptor>();
            services.AddSignalR();
            services.AddScoped<IRealtimeNotificationService, RealtimeNotificationService>();
            return services;
        }
    }
}
