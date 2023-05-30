using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Hodl.Api.ViewModels.CurrencyModels;

public class CryptoCurrencyDetailViewModel
{
    [Key]
    public Guid Id { get; set; }

    public byte Decimals { get; set; } = 9;

    public string Symbol { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public bool Active { get; set; }

    public bool IsStableCoin { get; set; }

    public bool IsLocked { get; set; }

    [JsonIgnore]
    public Guid? ListingCryptoId { get; set; }

    public CryptoCurrencyListViewModel ListingReference { get; set; }

    public ListingViewModel Listing { get; set; }

    public virtual ICollection<ListingViewModel> Listings { get; set; }
}
