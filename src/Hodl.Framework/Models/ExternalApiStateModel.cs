using Hodl.Constants;

namespace Hodl.Models;

public class ExternalApiStateModel
{
    public string AppMutex { get; set; }

    public DateTime TimeStamp { get; set; }

    public ExternalApiState State { get; set; }

    public int StatusCode { get; set; }

    public string ErrorMessage { get; set; }
}
