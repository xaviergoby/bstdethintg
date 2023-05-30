using Microsoft.Extensions.Localization;
using System.Text.Json;

namespace Hodl.Api.Utils.Errors;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;
    private readonly IStringLocalizer<ErrorHandlingMiddleware> _localizer;

    public ErrorHandlingMiddleware(
        RequestDelegate next,
        IStringLocalizer<ErrorHandlingMiddleware> localizer,
        ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
        _localizer = localizer;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex, _localizer, _logger);
        }
    }

    private static async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception,
        IStringLocalizer<ErrorHandlingMiddleware> localizer,
        ILogger<ErrorHandlingMiddleware> logger)
    {
        logger.LogError(exception, "{message}", exception.Message);

        var errors = new List<ErrorInformationItem>();

        if (exception is RestException restException)
        {
            context.Response.StatusCode = (int)restException.HttpCode;
            errors = restException.Errors;
        }
        else if (exception is AutoMapperMappingException mapperException)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            if (mapperException.MemberMap != null)
            {
                errors.Add(new ErrorInformationItem
                {
                    Field = mapperException.MemberMap.DestinationName,
                    Code = ErrorCodesStore.InvalidParameter,
                    Description = mapperException.InnerException.Message
                });
            }
            else
            {
                errors.Add(new ErrorInformationItem
                {
                    Code = ErrorCodesStore.InvalidParameter,
                    Description = mapperException.Message
                });
            }
        }
        else
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            errors.Add(new ErrorInformationItem
            {
                Code = ErrorCodesStore.InternalServerError,
                Description = localizer[ErrorCodesStore.InternalServerError].Value
            });
        }

        if (errors.Count > 0)
        {
            var content = JsonSerializer.Serialize(
                new { errors },
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(content);
        }
    }
}
