namespace Hodl.Api.Interfaces;

public interface IDiscordService
{
    Task MessageChannel(string message);
}
