using System.ComponentModel.DataAnnotations;

namespace Hodl.Api.ViewModels.IdentityModels;

public class UserLoginViewModel
{
    [Required(ErrorMessage = "E-mail address is required")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    public string Password { get; set; }
}
