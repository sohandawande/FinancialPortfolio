using FinancialPortfolio.Data.Context;
using FinancialPortfolio.Data.Interceptors;
using FinancialPortfolio.Models.Model.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FinancialPortfolio.Data.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDataLayer(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<SeedSettings>(configuration.GetSection(SeedSettings.SectionName));
            services.AddDbContext<ApplicationDbContext>((sp, options) =>
            {
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    sql =>
                    {
                        sql.MigrationsAssembly(
                            typeof(ApplicationDbContext)
                                .Assembly
                                .GetName()
                                .Name);
                    });

                options.AddInterceptors(sp.GetRequiredService<AuditSaveChangesInterceptor>());
            });

            return services;
        }
    }
}
