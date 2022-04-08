using AutoMapper;

namespace KonbiCloud.Diagnostic.Dtos
{
    public class DiagnosticMapProfile : Profile
    {
        public DiagnosticMapProfile()
        {
            CreateMap<HardwareDiagnosticFromClientDto, HardwareDiagnostic>()
                .ForMember(x => x.OriginId, opt => opt.MapFrom(src=>src.Id))
                .ForMember(x => x.Id, opt => opt.Ignore())
                .ForMember(x => x.OriginCreatedDate, opt => opt.MapFrom(src => src.CreatedDate));
            CreateMap<HardwareDiagnosticDetailDto, HardwareDiagnosticDetail>()
                .ForMember(x => x.Id, opt => opt.Ignore())
                .ForMember(x => x.OriginId, opt => opt.MapFrom(src => src.Id));
        }
    }
}
