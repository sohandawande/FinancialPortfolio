using AutoMapper;
using FinancialPortfolio.Business.Common.Helpers;
using FinancialPortfolio.Data.Entities.Stock;
using FinancialPortfolio.Models.Model.Response.Stock;
using Microsoft.AspNetCore.Http;

namespace FinancialPortfolio.Business.Mapping.Resolver
{
    public sealed class PublicLogoUrlResolver : IValueResolver<StockDetailEntity, StocksResponse, string?>
    {
        private readonly IHttpContextAccessor _http;

        public PublicLogoUrlResolver(IHttpContextAccessor http)
        {
            _http = http;
        }

        public string? Resolve(
            StockDetailEntity source,
            StocksResponse destination,
            string? destMember,
            ResolutionContext context)
        {
            return LogoUrlHelper.ToPublicUrl(source.LogoUrl, _http.HttpContext?.Request);
        }
    }
}
