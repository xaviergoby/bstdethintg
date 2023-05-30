using Hodl.Api.ViewModels.CurrencyModels;
using Hodl.ExchangeAPI.Models;

namespace Hodl.Api.ViewModels.TradingModels;

public class TradingMappingProfile : Profile
{
    public TradingMappingProfile()
    {
        CreateMap<Transfer, TransferListViewModel>();
        CreateMap<Transfer, TransferDetailViewModel>();
        CreateMap<PagingModel<Transfer>, PagingViewModel<TransferListViewModel>>();
        CreateMap<TransferAddViewModel, Transfer>()
             .ForMember(dest => dest.HoldingId, opt => opt.MapFrom(src => src.FromHoldingId))
             .ForMember(dest => dest.TransactionSource, opt => opt.MapFrom(src => src.FromAddress));
        CreateMap<TransferEditViewModel, Transfer>()
             .ForMember(dest => dest.TransactionSource, opt => opt.MapFrom(src => src.FromAddress));

        CreateMap<Order, OrderListViewModel>()
            .ForMember(dest => dest.Exchange, opt => opt.MapFrom(src => src.ExchangeAccount.Exchange.ExchangeName))
            .ForMember(dest => dest.ExchangeAccount, opt => opt.MapFrom(src => src.ExchangeAccount.Name))
            .ForMember(dest => dest.Executed, opt => opt.MapFrom(src => src.Trades == null ? 0 : src.Trades.Sum(t => t.Executed)))
            .ForMember(dest => dest.TotalCost, opt => opt.MapFrom(src => src.Trades == null ? 0 : src.Trades.Sum(t => t.Total)))
            .ForMember(dest => dest.AveragePrice, opt => opt.MapFrom(src => src.Trades != null && src.Trades.Count > 0
                ? src.Trades.Sum(t => t.Total) / src.Trades.Sum(t => t.Executed)
                : src.UnitPrice))
            .ForMember(dest => dest.TotalFees, opt => opt.Ignore())
            .AfterMap((src, dest, context) =>
            {
                dest.TotalFees = SumFees(context, src.Trades);
            });
        CreateMap<Order, OrderDetailViewModel>()
            .ForMember(dest => dest.ImportedFromExchange, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.ExchangeOrderId)))
            .ForMember(dest => dest.Executed, opt => opt.MapFrom(src => src.Trades == null ? 0 : src.Trades.Sum(t => t.Executed)))
            .ForMember(dest => dest.TotalCost, opt => opt.MapFrom(src => src.Trades == null ? 0 : src.Trades.Sum(t => t.Total)))
            .ForMember(dest => dest.AveragePrice, opt => opt.MapFrom(src => src.Trades != null && src.Trades.Count > 0
                ? src.Trades.Sum(t => t.Total) / src.Trades.Sum(t => t.Executed)
                : src.UnitPrice))
            .ForMember(dest => dest.TotalFees, opt => opt.Ignore())
            .AfterMap((src, dest, context) =>
            {
                dest.TotalFees = SumFees(context, src.Trades);
            });
        CreateMap<PagingModel<Order>, PagingViewModel<OrderListViewModel>>();
        CreateMap<OrderAddViewModel, Order>();
        CreateMap<OrderEditViewModel, Order>();

        CreateMap<OrderFunding, OrderFundingListViewModel>()
            .ForMember(dest => dest.FundName, opt => opt.MapFrom(src => src.Fund.FundName));
        CreateMap<PagingModel<OrderFunding>, PagingViewModel<OrderFundingListViewModel>>();
        CreateMap<OrderFundingEditViewModel, OrderFunding>();

        CreateMap<Trade, TradeListViewModel>();
        CreateMap<Trade, TradeDetailViewModel>();
        CreateMap<PagingModel<Trade>, PagingViewModel<TradeListViewModel>>();
        CreateMap<TradeAddViewModel, Trade>();

        CreateMap<Pair, PairViewModel>();
        CreateMap<PagingModel<Pair>, PagingViewModel<PairViewModel>>();

        CreateMap<ExchangeOrder, ExchangeOrderListViewModel>()
            .ForMember(dest => dest.BaseAsset, opt => opt.MapFrom(src => src.BaseAsset.Symbol))
            .ForMember(dest => dest.QuoteAsset, opt => opt.MapFrom(src => src.QuoteAsset.Symbol))
            .ForMember(dest => dest.Direction, opt => opt.MapFrom(src => (OrderDirection)src.Direction))
            .ForMember(dest => dest.State, opt => opt.MapFrom(src => (OrderState)src.State));
    }

    private static ICollection<FeeSumViewModel> SumFees(ResolutionContext context, ICollection<Trade> trades)
    {
        Dictionary<Guid, FeeSumViewModel> sums = new();

        if (trades != null)
        {
            foreach (var trade in trades)
            {
                if (sums.ContainsKey(trade.FeeCurrencyId))
                {
                    sums[trade.FeeCurrencyId].FeeSum += trade.Fee;
                }
                else
                {
                    sums[trade.FeeCurrencyId] = new FeeSumViewModel()
                    {
                        FeeSum = trade.Fee,
                        FeeCurrency = context.Mapper.Map<CryptoCurrencyListViewModel>(trade.FeeCurrency)
                    };
                }
            }
        }

        return sums.Values;
    }
}
