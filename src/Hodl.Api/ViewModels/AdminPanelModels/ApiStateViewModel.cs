namespace Hodl.Api.ViewModels.AdminPanelModels;

public class ApiStateViewModel
{
    public string ApiName { get; set; }

    [VisibleForRoles(Roles = "Admin")]
    public string AppMutex { get; set; }

    public DateTime TimeStamp { get; set; }

    public ExternalApiState State { get; set; }

    public int StatusCode { get; set; }

    [VisibleForRoles(Roles = "Admin")]
    public string ErrorMessage { get; set; }
}
