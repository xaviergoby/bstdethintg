using System.ComponentModel.DataAnnotations;

namespace Hodl.Api.ViewModels.IdentityModels;

public class UserUpdatePasswordModelView
{
    [Required(ErrorMessage = "The new password can not be empty")]
    public string NewPassword { get; set; }

    [Required(ErrorMessage = "The old password is required change the password")]
    public string OldPassword { get; set; }
}
