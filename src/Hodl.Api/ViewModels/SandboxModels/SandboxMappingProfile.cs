namespace Hodl.Api.ViewModels.SandboxModels;

public class SandboxMappingProfile : Profile
{
    public SandboxMappingProfile()
    {
        CreateMap<Holding, SandboxHoldingViewModel>();
        //.ForMember(dest => dest.IsLocked, opt => opt.MapFrom(src => src.Currency == null && src.CryptoCurrency.IsLocked));

    }
}
