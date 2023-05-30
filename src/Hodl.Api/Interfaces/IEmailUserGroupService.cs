namespace Hodl.Api.Interfaces;

public interface IEmailUserGroupService
{
    Task SendAlert(string userRole, string subject, string message, CancellationToken cancellationToken = default);

    void ResetErrorFor(string task, string sourceId);

    bool IsRegisteredAsError(string task, string sourceId);

    void RegisterErrorFor(string task, string sourceId);
}
