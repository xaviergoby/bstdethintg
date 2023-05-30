using System.ComponentModel.DataAnnotations;

namespace Hodl.Api.ViewModels.FundModels;

public class HoldingAddViewModel
{
    [MaxLength(3)]
    public string CurrencyISOCode { get; set; }

    public Guid? CryptoId { get; set; }

    public Guid? SharesFundId { get; set; }

    public DateTime StartDateTime { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader")]
    public decimal StartBalance { get; set; }

    public decimal StartUSDPrice { get; set; }

    public decimal StartBTCPrice { get; set; }

    public DateTime? EndDateTime { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader")]
    public decimal EndBalance { get; set; }

    public decimal EndUSDPrice { get; set; }

    public decimal EndBTCPrice { get; set; }

    public byte LayerIndex { get; set; }
}
