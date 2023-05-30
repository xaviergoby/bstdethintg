using Hodl.ExplorerAPI.Models;
using Hodl.MarketAPI.Models;

namespace Hodl.Api.Utils.Mappers;

public class ExplorerApiMappingProfile : Profile
{
    public ExplorerApiMappingProfile()
    {
        // <TSource, TDestination>
        CreateMap<Transaction, Order>();
    }
}


