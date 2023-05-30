using System.ComponentModel.DataAnnotations;

namespace Hodl.Api.ViewModels.IdentityModels;

public class UserUpdateEmailModelView
{
    [Required(ErrorMessage = "Email address is required")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Password is required to modify the email address")]
    public string Password { get; set; }
}
