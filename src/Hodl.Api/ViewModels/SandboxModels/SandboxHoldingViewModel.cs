using Hodl.Api.ViewModels.CurrencyModels;
using Hodl.Api.ViewModels.FundModels;

namespace Hodl.Api.ViewModels.SandboxModels;

public class SandboxHoldingViewModel
{
    public CryptoCurrencyListViewModel CryptoCurrency { get; set; }

    public Listing Listing { get; set; }

    public decimal EndBalance { get; set; }

    public decimal EndUSDPrice { get; set; }

    public decimal EndBTCPrice { get; set; }

    public decimal EndPercentage { get; set; }

    public byte LayerIndex { get; set; }

    public FundLayerViewModel FundLayer { get; set; }

    public IEnumerable<CategoryViewModel> Categories { get; set; }

    public decimal EntryAmount { get; set; }

    public decimal EntryTotal { get; set; }

    public decimal AvgEntryPrice { get => EntryAmount == 0 ? 0 : EntryTotal / EntryAmount; }

    public decimal ExitAmount { get; set; }

    public decimal ExitTotal { get; set; }

    public decimal AvgExitPrice { get => ExitAmount == 0 ? 0 : ExitTotal / ExitAmount; }
}
