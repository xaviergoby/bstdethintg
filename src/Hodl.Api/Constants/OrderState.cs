namespace Hodl.Api.Constants;

public enum OrderState
{
    Unknown = 0,
    New = 1,
    Submitted = 2,
    TriggerPending = 3,
    Active = 4,
    ChangePending = 5,
    CancelPending = 6,
    Cancelled = 7,
    Rejected = 8,
    PartFilled = 9,
    Filled = 10,
    Expired = 11
}
