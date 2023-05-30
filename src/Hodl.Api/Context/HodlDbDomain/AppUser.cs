namespace Hodl.Api.HodlDbDomain;

public class AppUser : IdentityUser<Guid>
{
    public AppUser()
    {
        Id = Guid.NewGuid();
    }

    public IList<string> Roles;
}

public class AppRole : IdentityRole<Guid>
{
    public AppRole()
    {
        Id = Guid.NewGuid();
    }
}
