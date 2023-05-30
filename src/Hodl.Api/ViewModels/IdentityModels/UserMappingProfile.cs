namespace Hodl.Api.ViewModels.IdentityModels;

public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<AppUser, UserViewModel>()
            .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.Roles.ToArray()));
        CreateMap<AppUser, UserListViewModel>()
            .ForMember(dest => dest.LockoutEnabled, opt => opt.MapFrom(src => src.LockoutEnd.Equals(DateTimeOffset.MaxValue)));
        CreateMap<UserToken, UserViewModel>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
            .ForMember(dest => dest.EmailConfirmed, opt => opt.MapFrom(src => src.User.EmailConfirmed))
            .ForMember(dest => dest.TwoFactorEnabled, opt => opt.MapFrom(src => src.User.TwoFactorEnabled))
            .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.User.Roles.ToArray()))
            .ForMember(dest => dest.Token, opt => opt.MapFrom(src => src.Token.Value))
            .ForMember(dest => dest.ExpirationTime, opt => opt.MapFrom(src => new DateTimeOffset(src.Token.Expiration).ToUnixTimeSeconds().ToString()));
    }
}
