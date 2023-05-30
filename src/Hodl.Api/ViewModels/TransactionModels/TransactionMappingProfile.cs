using Hodl.ExplorerAPI.Models;

namespace Hodl.Api.ViewModels.TransactionModels;

public class TransactionMappingProfile : Profile
{
    public TransactionMappingProfile()
    {
        // NormalTransaction
        // <TSource, TDestination>
        CreateMap<Transaction, TransactionDetailViewModel>();
        CreateMap<TransactionDetailViewModel, Transaction>();

        CreateMap<Transaction, TransactionListViewModel>()
            .ForMember(dest => dest.EndUSDValue, opt => opt.MapFrom(src => src.EndBalance * src.EndUSDPrice))
            .ForMember(dest => dest.EndBTCValue, opt => opt.MapFrom(src => src.EndBalance * src.EndBTCPrice));
    }
}


