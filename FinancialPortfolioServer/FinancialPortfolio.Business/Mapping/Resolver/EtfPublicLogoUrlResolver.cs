using AutoMapper;
using FinancialPortfolio.Business.Common.Helpers;
using FinancialPortfolio.Data.Entities.Etf;
using FinancialPortfolio.Models.Model.Response.Etf;
using Microsoft.AspNetCore.Http;

namespace FinancialPortfolio.Business.Mapping.Resolver
{
    /// <summary>
    /// Same behavior as PublicLogoUrlResolver, but typed for ETF entities.
    /// AutoMapper IValueResolver is closed over source/dest types, so Stock resolver cannot be reused.
    /// </summary>
    public sealed class EtfPublicLogoUrlResolver : IValueResolver<EtfDetailEntity, EtfsResponse, string?>
    {
        private readonly IHttpContextAccessor _http;

        public EtfPublicLogoUrlResolver(IHttpContextAccessor http)
        {
            _http = http;
        }

        public string? Resolve(
            EtfDetailEntity source,
            EtfsResponse destination,
            string? destMember,
            ResolutionContext context)
        {
            return LogoUrlHelper.ToPublicUrl(source.LogoUrl, _http.HttpContext?.Request);
        }
    }
}
