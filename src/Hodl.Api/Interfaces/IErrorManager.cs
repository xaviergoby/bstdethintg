namespace Hodl.Api.Interfaces;

public interface IErrorManager
{
    HttpStatusCode StatusCode { get; set; }

    void AddValidationError(HttpStatusCode httpCode = HttpStatusCode.InternalServerError, string description = null, string code = null, string field = null);
    void AddValidationError(HttpStatusCode httpCode, ErrorInformationItem error);
    void ThrowOnErrors();
}