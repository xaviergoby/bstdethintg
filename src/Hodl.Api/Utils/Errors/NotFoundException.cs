namespace Hodl.Api.Utils.Errors;

public class NotFoundException : RestException
{
    public NotFoundException(string message) : base(HttpStatusCode.NotFound, message)
    {
    }
}
