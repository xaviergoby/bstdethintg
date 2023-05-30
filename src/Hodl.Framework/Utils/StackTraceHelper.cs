using System.Runtime.CompilerServices;

namespace Hodl.Framework.Utils;

public static class StackTraceHelper
{
    public static string CallingMethod(object caller, [CallerMemberName] string membername = "")
    {
        return $"{caller?.GetType()?.Name}.{membername}"; ;
    }
}
