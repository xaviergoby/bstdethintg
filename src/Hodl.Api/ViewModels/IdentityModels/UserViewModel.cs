namespace Hodl.Api.ViewModels.IdentityModels;

public record UserViewModel
{
    public string Email { get; set; }

    public bool EmailConfirmed { get; set; }

    public bool TwoFactorEnabled { get; set; }

    public string Token { get; set; }

    public string ExpirationTime { get; set; }

    public string[] Roles { get; set; }
}
