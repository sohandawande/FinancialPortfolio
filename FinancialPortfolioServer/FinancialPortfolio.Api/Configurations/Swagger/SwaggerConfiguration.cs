namespace FinancialPortfolio.Api.Configurations.Swagger
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.OpenApi.Models;
    using System;
    using System.IO;

    /// <summary>
    /// Defines the <see cref="SwaggerConfiguration" />
    /// </summary>
    public static class SwaggerConfiguration
    {
        /// <summary>
        /// The AddSwaggerConfiguration
        /// </summary>
        /// <param name="services">The services<see cref="IServiceCollection"/></param>
        /// <returns>The <see cref="IServiceCollection"/></returns>
        public static IServiceCollection AddSwaggerConfiguration(
            this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Financial Portfolio API",
                    Version = "v1",
                    Description = "Financial Portfolio Management API"
                });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter JWT token.\n\nExample: Bearer eyJhbGciOi..."
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

                foreach (var xmlFile in Directory.GetFiles(AppContext.BaseDirectory, "*.xml"))
                {
                    options.IncludeXmlComments(xmlFile, includeControllerXmlComments: true);
                }
            });

            return services;
        }
    }
}
