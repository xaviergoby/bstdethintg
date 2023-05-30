namespace Hodl.Api.Utils.Configurations;

public class EmailOptions
{
    public string Host { get; set; } = "smtp.gmail.com";

    public int Port { get; set; } = 587;

    public bool EnableSsl { get; set; } = true;

    public string Email { get; set; }

    public string Password { get; set; }
}
