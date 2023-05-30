namespace Hodl.Api.Utils.Errors;

public class ErrorManager : IErrorManager
{
    private readonly List<ErrorInformationItem> _items = new();

    private HttpStatusCode _statusCode = HttpStatusCode.OK;

    public HttpStatusCode StatusCode
    {
        get { return _statusCode; }
        set
        {
            // Only set the statuscode ones, on the first error detected
            if (_statusCode == HttpStatusCode.OK)
                _statusCode = value;
        }
    }

    public void AddValidationError(HttpStatusCode httpCode = HttpStatusCode.InternalServerError, string description = null, string code = null, string field = null)
    {
        AddValidationError(httpCode, new ErrorInformationItem
        {
            Description = description,
            Code = code,
            Field = field,
        });
    }

    public void AddValidationError(HttpStatusCode httpCode, ErrorInformationItem error)
    {
        StatusCode = httpCode;
        _items.Add(error);
    }

    public void ThrowOnErrors()
    {
        if (_statusCode == HttpStatusCode.OK) return;

        // There is an error code registered, so raise the error
        throw new RestException(StatusCode, _items);
    }
}
