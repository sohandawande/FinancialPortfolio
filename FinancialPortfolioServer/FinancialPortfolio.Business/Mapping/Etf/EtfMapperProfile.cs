using AutoMapper;
using FinancialPortfolio.Business.Mapping.Resolver;
using FinancialPortfolio.Data.Entities.Etf;
using FinancialPortfolio.Models.Model.Request.Etf;
using FinancialPortfolio.Models.Model.Response.Etf;

namespace FinancialPortfolio.Business.Mapping.Etf
{
    public class EtfMapperProfile : Profile
    {
        public EtfMapperProfile()
        {
            // ETF Create Request
            CreateMap<EtfCreateRequest, EtfDetailEntity>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.EtfId, o => o.Ignore())
                .ForMember(d => d.Etf, o => o.Ignore());

            CreateMap<EtfCreateRequest, EtfEntity>()
                .ForMember(d => d.EtfDetail, o => o.MapFrom(s => s));

            // ETF Update Request
            CreateMap<EtfUpdateRequest, EtfDetailEntity>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.EtfId, o => o.Ignore())
                .ForMember(d => d.Etf, o => o.Ignore());

            CreateMap<EtfUpdateRequest, EtfEntity>()
                .ForMember(d => d.EtfDetail, o => o.MapFrom(s => s));

            // ETF Response
            CreateMap<EtfDetailEntity, EtfsResponse>()
                .ForMember(d => d.CreatedDate, o => o.Ignore())
                .ForMember(d => d.CreatedBy, o => o.Ignore())
                .ForMember(d => d.ModifiedDate, o => o.Ignore())
                .ForMember(d => d.ModifiedBy, o => o.Ignore())
                .ForMember(d => d.LogoUrl, o => o.MapFrom<EtfPublicLogoUrlResolver>());

            CreateMap<EtfEntity, EtfsResponse>().IncludeMembers(x => x.EtfDetail);
        }
    }
}
