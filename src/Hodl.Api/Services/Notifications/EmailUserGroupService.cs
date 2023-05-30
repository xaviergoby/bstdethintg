using Microsoft.Extensions.Options;
using System.Reflection;

namespace Hodl.Api.Services.Notifications;

public class EmailUserGroupService : IEmailUserGroupService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IEmailService _emailService;
    private readonly string _runningEnvironment;

    private static readonly Dictionary<string, HashSet<string>> registeredErrors = new();

    public EmailUserGroupService(
        IOptions<AppDefaults> settings,
        UserManager<AppUser> userManager,
        IEmailService emailService)
    {
        _userManager = userManager;
        _emailService = emailService;
        _runningEnvironment = settings.Value.RunningEnvironment;
    }

    public async Task SendAlert(string userRole, string subject, string message, CancellationToken cancellationToken = default)
    {
        var users = await _userManager.GetUsersInRoleAsync(userRole);

        if (userRole.Equals("Admin", StringComparison.InvariantCultureIgnoreCase))
        {
            message = AddAppInfo(message);
        }

        List<Task> tasks = new();
        foreach (var user in users)
        {
            tasks.Add(_emailService.SendAsync(user.Email, subject, message, cancellationToken));
        }

        Task.WaitAll(tasks.ToArray(), cancellationToken);
    }

    public void ResetErrorFor(string task, string sourceId)
    {
        lock (registeredErrors)
        {
            if (registeredErrors.ContainsKey(task))
            {
                registeredErrors[task].Remove(sourceId);
            }
        }
    }

    public bool IsRegisteredAsError(string task, string sourceId)
    {
        lock (registeredErrors)
        {
            if (registeredErrors.ContainsKey(task))
            {
                return registeredErrors[task].Contains(sourceId);
            }
        }

        return false;
    }

    public void RegisterErrorFor(string task, string sourceId)
    {
        lock (registeredErrors)
        {
            if (!registeredErrors.ContainsKey(task))
            {
                registeredErrors[task] = new HashSet<string>();
            }
            registeredErrors[task].Add(sourceId);
        }
    }

    private string AddAppInfo(string message)
    {
        string pattern = @"{0}<br />
<br />
Sent from:<br />
    Environment:            {1}<br />
    APIVersion:             {2}<br />
    BuildDate:              {3}<br />
    MachineName:            {4}<br />
    RunMutex:               {5}<br />
    ProcessId:              {6}<br />
    LocalTimeZone:          {7}<br />
    CurrentLocalDateTime:   {8}<br />
    CurrentUTCDateTime:     {9}<br />
";
        Assembly assembly = Assembly.GetExecutingAssembly();

        return string.Format(pattern, message,
            _runningEnvironment,
            assembly.GetName().Version.ToString(),
            File.GetLastWriteTime(assembly.Location).ToShortDateString(),
            Environment.MachineName,
            AppMutex.Mutex,
            Environment.ProcessId,
            TimeZoneInfo.Local.DisplayName,
            DateTimeOffset.Now,
            DateTimeOffset.UtcNow
            );
    }
}
