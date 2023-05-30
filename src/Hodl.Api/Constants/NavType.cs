using System.Runtime.Serialization;

namespace Hodl.Api.Constants;

public enum NavType
{
    [EnumMember(Value = "PERIOD")]
    Period,

    [EnumMember(Value = "DAILY")]
    Daily,
}
