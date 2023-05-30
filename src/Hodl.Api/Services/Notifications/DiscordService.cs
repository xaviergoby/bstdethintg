using Discord.Webhook;
using Microsoft.Extensions.Options;

namespace Hodl.Api.Services.Notifications;

public class DiscordService : IDiscordService
{
    private readonly DiscordOptions _discordOptions;

    public DiscordService(IOptions<DiscordOptions> discordSettings)
    {
        _discordOptions = discordSettings.Value;
    }

    public async Task MessageChannel(string message)
    {
        if (!string.IsNullOrEmpty(_discordOptions.WebhookUrl) &&
            Uri.TryCreate(_discordOptions.WebhookUrl, UriKind.Absolute, out _))
        {
            var webhookClient = new DiscordWebhookClient(_discordOptions.WebhookUrl);
            await webhookClient.SendMessageAsync(message);
        }
    }
}
