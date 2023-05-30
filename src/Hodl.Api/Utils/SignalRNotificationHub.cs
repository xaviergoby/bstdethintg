using Microsoft.AspNetCore.SignalR;


namespace Hodl.Api.Utils;

[Authorize(AuthenticationSchemes = JwtIssuerOptions.Schemes)]
public class SignalRNotificationHub : Hub
{
    private readonly IUserResolver _userResolver;


    public SignalRNotificationHub(IUserResolver userResolver)
    {
        _userResolver = userResolver;
    }


    public override async Task OnConnectedAsync()
    {
        var user = await _userResolver.GetUser();
        foreach (var role in user?.Roles)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, role);
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var user = await _userResolver.GetUser();
        foreach (var role in user?.Roles)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, role);
        }
        await base.OnDisconnectedAsync(exception);
    }

}




