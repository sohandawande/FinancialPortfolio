using AutoMapper;
using FinancialPortfolio.Data.Entities.Portfolio;
using FinancialPortfolio.Models.Model.Request.Portfolio;

namespace FinancialPortfolio.Business.Mapping.Portfolio
{
    public class PortfolioMappingProfile : Profile
    {
        public PortfolioMappingProfile()
        {
            // We mostly use manual mapping because of calculated fields,
            // but keep these for future flexibility.

            CreateMap<BuyStockRequest, PortfolioStockHoldEntity>()
                .ForMember(d => d.RemainingQuantity, opt => opt.MapFrom(s => s.Quantity))
                .ForMember(d => d.InvestmentAmount, opt => opt.MapFrom(s => s.Quantity * s.PurchasePrice))
                .ForMember(d => d.LotStatus, opt => opt.MapFrom(_ => Models.Common.Enums.LotStatus.Open))
                .ForMember(d => d.IsSold, opt => opt.MapFrom(_ => false));
        }
    }
}
