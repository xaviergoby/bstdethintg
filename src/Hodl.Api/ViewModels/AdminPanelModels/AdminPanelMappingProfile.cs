namespace Hodl.Api.ViewModels.AdminPanelModels;

public class AdminPanelMappingProfile : Profile
{
    public AdminPanelMappingProfile()
    {
        CreateMap<ExternalApiStateModel, ApiStateViewModel>();
    }
}
