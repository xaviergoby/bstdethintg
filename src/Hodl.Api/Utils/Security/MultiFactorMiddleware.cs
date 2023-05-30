namespace Hodl.Api.Utils.Security;

public class MultiFactorMiddleware : IMiddleware
{
    private readonly IUserResolver _userResolver;
    public MultiFactorMiddleware(IUserResolver userResolver)
    {
        _userResolver = userResolver;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.GetEndpoint()?.Metadata?.GetMetadata<AuthorizeAttribute>() is not null)
        {
            var user = await _userResolver.GetUser();
            if (user is not null && user.TwoFactorEnabled && !await _userResolver.IsMultiFactorVerified())
            {
                // Return unauthorized, and message for MFA redirect
                throw new RestException(HttpStatusCode.Unauthorized,
                    new ErrorInformationItem
                    {
                        Code = ErrorCodesStore.RequiresMfaCode,
                        Description = "Use your authenticator to fill the multi factor authentication code.",
                    });
            }
        }

        await next(context);
    }
}