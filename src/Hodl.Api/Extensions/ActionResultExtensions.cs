namespace Hodl.Api.Extensions;

public static class ActionResultExtensions
{
    public static ObjectResult GetErrorResult(this ErrorInformationItem error, HttpStatusCode httpStatusCode = HttpStatusCode.BadRequest)
    {
        var errors = new List<ErrorInformationItem> { error };

        return errors.GetErrorResult(httpStatusCode);
    }

    public static ObjectResult GetErrorResult(this IEnumerable<ErrorInformationItem> errors, HttpStatusCode httpStatusCode = HttpStatusCode.BadRequest)
    {
        return GetObjectResult(new { errors }, httpStatusCode);
    }

    private static ObjectResult GetObjectResult(object model, HttpStatusCode httpStatusCode)
    {
        return new ObjectResult(model)
        {
            StatusCode = (int)httpStatusCode
        };
    }
}
