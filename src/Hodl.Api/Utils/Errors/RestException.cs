using System.Text.Json;

namespace Hodl.Api.Utils.Errors;

public class RestException : Exception
{
    public HttpStatusCode HttpCode { get; }

    public List<ErrorInformationItem> Errors { get; } = new();

    public override string Message => JsonSerializer.Serialize(Errors);

    public RestException(HttpStatusCode httpCode = HttpStatusCode.InternalServerError, string description = null, string code = null, string field = null)
    {
        HttpCode = httpCode;

        if (!string.IsNullOrEmpty(description) ||
            !string.IsNullOrEmpty(code) ||
            !string.IsNullOrEmpty(field))
        {
            Errors.Add(new()
            {
                Field = field,
                Code = code,
                Description = description
            });
        }
    }

    public RestException(HttpStatusCode httpCode, IEnumerable<ErrorInformationItem> errors)
    {
        HttpCode = httpCode;

        Errors = errors != null
            ? errors.ToList()
            : new List<ErrorInformationItem>();
    }

    public RestException(HttpStatusCode httpCode, ErrorInformationItem error)
    {
        HttpCode = httpCode;

        Errors = new List<ErrorInformationItem>
            {
                error
            };
    }
}
