using System.Runtime.Serialization;

namespace Hodl.Api.Constants;

public enum TransactionType
{
    [EnumMember(Value = "WALLET")]
    Wallet,

    [EnumMember(Value = "BANK")]
    Bank,

    [EnumMember(Value = "BROKER")]
    Broker,

    [EnumMember(Value = "TRANSFER")]
    Transfer,

    [EnumMember(Value = "INFLOW")]
    Inflow,

    [EnumMember(Value = "OUTFLOW")]
    Outflow,

    [EnumMember(Value = "REWARD")]
    Reward,

    [EnumMember(Value = "PROFIT")]
    Profit,

    [EnumMember(Value = "LOSS")]
    Loss,

    [EnumMember(Value = "ADMIN_FEE")]
    AdministrationFee,

    [EnumMember(Value = "PERF_FEE")]
    PerformanceFee,

    [EnumMember(Value = "CORRECTION")]
    Correction
}
