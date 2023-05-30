using System.ComponentModel.DataAnnotations;

namespace Hodl.Api.ViewModels.IdentityModels;

public class UserEmailConfirmViewModel
{
    [Required(ErrorMessage = "E-mail address is required")]
    public string Email { get; set; }

    [Required(ErrorMessage = "The verification code is required")]
    public string VerifyCode { get; set; }
}
