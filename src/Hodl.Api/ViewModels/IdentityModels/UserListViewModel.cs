using System.ComponentModel.DataAnnotations;

namespace Hodl.Api.ViewModels.IdentityModels;

public class UserListViewModel
{
    [Key]
    public Guid Id { get; set; }

    public string Email { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader,HeadSales")]
    public bool EmailConfirmed { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader,HeadSales")]
    public bool TwoFactorEnabled { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader,HeadSales")]
    public bool LockoutEnabled { get; set; }

    public string[] Roles { get; set; }
}
