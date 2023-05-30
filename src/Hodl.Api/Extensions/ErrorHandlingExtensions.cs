namespace Hodl.Api.Extensions;

public static class ErrorHandlingExtensions
{
    public static IEnumerable<ErrorInformationItem> CastAsErrorInformationItems(this IEnumerable<IdentityError> identityErrors)
    {
        var result = new List<ErrorInformationItem>();

        if (identityErrors == null)
            return result;

        foreach (var identityError in identityErrors)
        {
            result.Add(new ErrorInformationItem
            {
                Code = identityError.Code,
                Description = identityError.Description
            });
        }

        return result;
    }
}
