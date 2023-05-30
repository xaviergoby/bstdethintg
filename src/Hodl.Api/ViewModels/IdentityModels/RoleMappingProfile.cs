namespace Hodl.Api.ViewModels.IdentityModels;

public class RoleMappingProfile : Profile
{
    public RoleMappingProfile()
    {
        CreateMap<AppRole, RoleViewModel>();
    }
}
