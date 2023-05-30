using Hodl.Constants;
using Hodl.Models;
using Hodl.Utils;
using System.Net;

namespace Hodl.Extensions;

public static class HttpRequestExceptionExtensions
{
    public static ExternalApiStateModel GetApiState(this HttpRequestException ex) =>
        ex.StatusCode switch
        {
            HttpStatusCode.ServiceUnavailable => new ExternalApiStateModel
            {
                AppMutex = AppMutex.Mutex,
                TimeStamp = DateTime.UtcNow,
                State = ExternalApiState.Offline,
                StatusCode = (int)ex.StatusCode,
                ErrorMessage = ex.Message
            },
            _ => new ExternalApiStateModel
            {
                AppMutex = AppMutex.Mutex,
                TimeStamp = DateTime.UtcNow,
                State = ExternalApiState.Error,
                StatusCode = (int)ex.StatusCode,
                ErrorMessage = ex.Message
            }
        };
}
