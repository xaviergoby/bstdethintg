namespace Hodl.Api.Utils.Configurations;

public class JwtOptions
{
    public string SecurityKey { get; set; }

    public int ValidForMinutes { get; set; } = 60;
}
