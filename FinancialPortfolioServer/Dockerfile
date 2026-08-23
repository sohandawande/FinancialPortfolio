# ============================================================
# FinancialPortfolio.Api - Multi-stage Dockerfile (.NET 10)
# ============================================================

# ---------- Build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY FinancialPortfolioServer.slnx ./
COPY FinancialPortfolio.Api/FinancialPortfolio.Api.csproj                 FinancialPortfolio.Api/
COPY FinancialPortfolio.Business/FinancialPortfolio.Business.csproj       FinancialPortfolio.Business/
COPY FinancialPortfolio.Data/FinancialPortfolio.Data.csproj               FinancialPortfolio.Data/
COPY FinancialPortfolio.Models/FinancialPortfolio.Models.csproj           FinancialPortfolio.Models/
COPY FinancialPortfolio.QueryEngine/FinancialPortfolio.QueryEngine.csproj FinancialPortfolio.QueryEngine/

RUN dotnet restore FinancialPortfolio.Api/FinancialPortfolio.Api.csproj

COPY . .

# Force Unix line endings
RUN sed -i 's/\r$//' entrypoint.sh && chmod +x entrypoint.sh

WORKDIR /src/FinancialPortfolio.Api
RUN dotnet publish -c Release -o /app/publish --no-restore

# ---------- Runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

RUN groupadd -r appgroup && useradd -r -g appgroup -u 1001 appuser

COPY --from=build /app/publish .
COPY --from=build /src/entrypoint.sh /app/entrypoint.sh

RUN chmod +x /app/entrypoint.sh && chown appuser:appgroup /app/entrypoint.sh

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

EXPOSE 8080

USER appuser

ENTRYPOINT ["/app/entrypoint.sh"]