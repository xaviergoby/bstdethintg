using System.Runtime.Serialization;

namespace Hodl.Api.Constants;

public enum TransferDirection
{
    [EnumMember(Value = "IN")]
    In,
    [EnumMember(Value = "OUT")]
    Out
}
