namespace Hodl.Api.Utils.Errors;

public class ErrorInformationItem
{
    public string Field { get; set; }

    public string Code { get; set; }

    public string Description { get; set; }

    public object Payload { get; set; }
}
