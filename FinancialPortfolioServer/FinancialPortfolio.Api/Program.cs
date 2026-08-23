using FinancialPortfolio.Api.DependencyInjection;
using FinancialPortfolio.Api.Extensions.Middleware;
using FinancialPortfolio.Api.Hubs;
using FinancialPortfolio.Business.DependencyInjection;
using FinancialPortfolio.Data.DependencyInjection;
using FinancialPortfolio.Data.Seed;
using FinancialPortfolio.QueryEngine.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddApiServiceLayer(builder.Configuration);
builder.Services.AddBusinessLayer(builder.Configuration);
builder.Services.AddDataLayer(builder.Configuration);
builder.Services.AddQueryEngineLayer();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularPolicy", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:4200",  // local ng serve
                "http://localhost:4300"   // Docker client
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

// Migrate + seed roles + optional admin (env / appsettings controlled)
await DatabaseSeeder.SeedAsync(app.Services);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(options =>
    {
        options.DocumentTitle = "Financial Portfolio API";
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Financial Portfolio API v1");
        options.DisplayRequestDuration();
    });
}

app.UseHttpsRedirection();

app.UseGlobalExceptionMiddleware();

app.UseCors("AngularPolicy");

app.UseStaticFiles();

app.UseAuthentication();

app.UseAuthorization();

app.MapHub<NotificationHub>("/hubs/notifications");

app.MapControllers();

app.Run();
