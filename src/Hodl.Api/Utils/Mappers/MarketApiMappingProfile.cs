using Hodl.MarketAPI.Models;

namespace Hodl.Api.Utils.Mappers;

public class MarketApiMappingProfile : Profile
{
    public MarketApiMappingProfile()
    {
        CreateMap<CryptoCurrency, MarketCryptoCurrency>();
        CreateMap<MarketListing, Listing>();
    }
}
