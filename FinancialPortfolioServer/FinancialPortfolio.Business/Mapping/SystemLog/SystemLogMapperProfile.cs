using AutoMapper;
using FinancialPortfolio.Data.Entities.SystemLog;
using FinancialPortfolio.Models.Model.Response.SystemLog;

namespace FinancialPortfolio.Business.Mapping.SystemLog
{
    public class SystemLogMapperProfile : Profile
    {
        public SystemLogMapperProfile()
        {
            // Detail entity → detail DTO
            CreateMap<SystemLogDetailEntity, SystemLogDetailResponse>();

            // Main entity → response (nested Detail)
            CreateMap<SystemLogEntity, SystemLogResponse>()
                .ForMember(d => d.LogLevel, o => o.MapFrom(s => s.LogLevel.ToString()))
                .ForMember(d => d.Detail, o => o.MapFrom(s => s.SystemLogDetail));
        }
    }
}
