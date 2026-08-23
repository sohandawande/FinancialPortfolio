using AutoMapper;
using FinancialPortfolio.Data.Entities.AppUser;
using FinancialPortfolio.Models.Model.Request.Authentication;

namespace FinancialPortfolio.Business.Mapping.Authentication
{
    public class AuthenticationMapperProfile : Profile
    {
        public AuthenticationMapperProfile()
        {
            CreateMap<RegisterRequest, AppUserEntity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())

            .ForMember(dest => dest.IdentityUserId, opt => opt.Ignore())

            .ForMember(d => d.UserCode, o => o.Ignore())

            .ForMember(dest => dest.IsActive, opt => opt.Ignore())

            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())

            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())

            .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())

            .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())

            .ForMember(dest => dest.ModifiedDate, opt => opt.Ignore())

            .ForMember(d => d.IdentityUser, o => o.Ignore());
        }
    }
}
