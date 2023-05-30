namespace Hodl.Api.Services.Notifications.NotificationModels;

public class FundBoundaryBreach
{
    public DateTime TimeStamp { get; set; }

    public Guid FundId { get; set; }

    public string ItemId { get; set; }

    public string FundName { get; set; }

    public string ItemName { get; set; }

    public SumRecord SumRecord { get; set; }

    public string BoundarySide { get; set; }

    public int BoundaryLevel { get; set; }

    public int CurrentLevel { get; set; }
}
