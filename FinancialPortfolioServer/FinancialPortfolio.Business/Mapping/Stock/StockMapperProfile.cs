using AutoMapper;
using FinancialPortfolio.Business.Mapping.Resolver;
using FinancialPortfolio.Data.Entities.Stock;
using FinancialPortfolio.Models.Model.Request.Stock;
using FinancialPortfolio.Models.Model.Response.Stock;

namespace FinancialPortfolio.Business.Mapping.Stock
{
    public class StockMapperProfile : Profile
    {
        public StockMapperProfile()
        {
            //Stock Create Request
            CreateMap<StockCreateRequest, StockDetailEntity>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.StockId, o => o.Ignore())
            .ForMember(d => d.Stock, o => o.Ignore());

            CreateMap<StockCreateRequest, StockEntity>()
            .ForMember(d => d.StockDetail, o => o.MapFrom(s => s));

            //Stock Update Request
            CreateMap<StockUpdateRequest, StockDetailEntity>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.StockId, o => o.Ignore())
            .ForMember(d => d.Stock, o => o.Ignore());

            CreateMap<StockUpdateRequest, StockEntity>()
            .ForMember(d => d.StockDetail, o => o.MapFrom(s => s));

            //Stock Create Response
            CreateMap<StockDetailEntity, StocksResponse>()
            .ForMember(d => d.CreatedDate, o => o.Ignore())
            .ForMember(d => d.CreatedBy, o => o.Ignore())
            .ForMember(d => d.ModifiedDate, o => o.Ignore())
            .ForMember(d => d.ModifiedBy, o => o.Ignore())
            .ForMember(d => d.LogoUrl, o => o.MapFrom<PublicLogoUrlResolver>());

            CreateMap<StockEntity, StocksResponse>().IncludeMembers(x => x.StockDetail);

            //For Example For reverse map
            //CreateMap<StockEntity, StockListResponse>().ReverseMap();
        }
    }
}
