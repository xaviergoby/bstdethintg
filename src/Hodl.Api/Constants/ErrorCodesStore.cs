namespace Hodl.Api.Constants;

public static class ErrorCodesStore
{
    public const string InternalServerError = "InternalServerError";
    public const string InvalidParameter = "InvalidParameter";
    public const string MapperException = "MapperException";

    public const string ReferencedRecordNotFound = "ReferencedRecordNotFound";
    public const string DuplicateKey = "DuplicateKey";
    public const string ItemIsReferenced = "ItemIsReferenced";
    public const string ItemIsLocked = "ItemIsLocked";
    public const string DoubleLinked = "DoubleLinked";
    public const string EmptyValue = "EmptyValue";
    public const string ValueCannotBeChanged = "ValueCannotBeChanged";
    public const string InvalidValue = "InvalidValue";
    public const string ReferenceCheckFailed = "ReferenceCheckFailed";

    public const string InvalidCredentials = "InvalidCredentials";
    public const string EmailNotSended = "EmailNotSended";
    public const string EmailNotConfirmed = "EmailNotConfirmed";
    public const string RequiresEmailConfirmation = "RequiresEmailConfirmation";
    public const string RequiresMfaCode = "RequiresMultiFactorCode";
    public const string AccountLocked = "AccountLocked";
    public const string UserNotAdded = "UserNotAdded";
    public const string InvalidToken = "InvalidToken";
    public const string UserMultiFactorAlreadyEnabled = "UserMultiFactorAlreadyEnabled";
    public const string UserNotGiven = "UserNotGiven";

    // Transfer specific
    public const string BookingPeriodClosed = "BookingPeriodClosed";
    public const string IllegalTransfer = "IllegalTransfer";

    // Order/Trade specific
    public const string OrderStateError = "OrderStateError";
    public const string ValueExceedsOrder = "ValueExceedsOrder";
}
