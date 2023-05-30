using System.Runtime.Serialization;

namespace Hodl.Constants;

public enum ExternalApiState
{
    [EnumMember(Value = "UNKNOWN")]
    Unknown,

    [EnumMember(Value = "ONLINE")]
    Online,

    [EnumMember(Value = "OFFLINE")]
    Offline,

    [EnumMember(Value = "FAILING")]
    Failing,

    [EnumMember(Value = "ERROR")]
    Error
}
