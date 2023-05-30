namespace Hodl.Api.Utils.Security;

public class JwtTokenManagerMiddleware : IMiddleware
{
    private readonly IJwtTokenManager _tokenManager;

    public JwtTokenManagerMiddleware(IJwtTokenManager tokenManager)
    {
        _tokenManager = tokenManager;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.GetEndpoint()?.Metadata?.GetMetadata<AuthorizeAttribute>() is not null &&
            !await _tokenManager.IsCurrentActiveToken())
        {
            // Token is revoked
            throw new RestException(HttpStatusCode.Unauthorized,
                new ErrorInformationItem
                {
                    Code = ErrorCodesStore.InvalidToken,
                    Description = "The used token is not valid, please login again.",
                    Payload = context.GetEndpoint()
                });
        }

        await next(context);
    }
}