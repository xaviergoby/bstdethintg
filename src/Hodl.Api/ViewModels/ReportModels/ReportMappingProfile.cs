namespace Hodl.Api.ViewModels.ReportModels;

public class ReportMappingProfile : Profile
{
    public ReportMappingProfile()
    {
        CreateMap<Fund, ReportFundSelectionViewModel>()
            .ForMember(dest => dest.FundOwner, opt => opt.MapFrom(src => src.FundOwner == null ? string.Empty : src.FundOwner.Name));

        CreateMap<Fund, ReportFundViewModel>();
        CreateMap<FundLayer, ReportFundLayerViewModel>();
        CreateMap<Nav, ReportPeriodNavViewModel>();
        CreateMap<CurrencyRate, ReportCurrencyRateViewModel>();
        CreateMap<Holding, ReportHoldingViewModel>()
            .ForMember(dest => dest.CurrencySymbol, opt => opt.MapFrom(src => src.Currency == null ? src.CryptoCurrency.Symbol : src.Currency.ISOCode))
            .ForMember(dest => dest.CurrencyName, opt => opt.MapFrom(src => src.Currency == null ? src.CryptoCurrency.Name : src.Currency.Name))
            .ForMember(dest => dest.IsFiat, opt => opt.MapFrom(src => src.Currency != null || src.CryptoCurrency.IsStableCoin))
            .ForMember(dest => dest.IsLocked, opt => opt.MapFrom(src => src.Currency == null && src.CryptoCurrency.IsLocked));
        CreateMap<Holding, ReportTradeSummaryViewModel>()
            .ForMember(dest => dest.CurrencySymbol, opt => opt.MapFrom(src => src.Currency == null ? src.CryptoCurrency.Symbol : src.Currency.ISOCode))
            .ForMember(dest => dest.CurrencyName, opt => opt.MapFrom(src => src.Currency == null ? src.CryptoCurrency.Name : src.Currency.Name))
            .ForMember(dest => dest.IsFiat, opt => opt.MapFrom(src => src.Currency != null || src.CryptoCurrency.IsStableCoin))
            .ForMember(dest => dest.IsLocked, opt => opt.MapFrom(src => src.Currency == null && src.CryptoCurrency.IsLocked));

        CreateMap<Transfer, ReportTransferListViewModel>()
            .ForMember(dest => dest.CurrencySymbol, opt => opt.MapFrom(src => src.Holding.Currency == null ? src.Holding.CryptoCurrency.Symbol : src.Holding.Currency.ISOCode))
            .ForMember(dest => dest.CurrencyName, opt => opt.MapFrom(src => src.Holding.Currency == null ? src.Holding.CryptoCurrency.Name : src.Holding.Currency.Name))
            .ForMember(dest => dest.FeeCurrencySymbol, opt => opt.MapFrom(src => src.FeeHolding.Currency == null ? src.FeeHolding.CryptoCurrency.Symbol : src.FeeHolding.Currency.ISOCode))
            .ForMember(dest => dest.FeeCurrencyName, opt => opt.MapFrom(src => src.FeeHolding.Currency == null ? src.FeeHolding.CryptoCurrency.Name : src.FeeHolding.Currency.Name));

        CreateMap<Order, ReportOrderLogViewModel>()
            .ForMember(dest => dest.ExchangeAccount, opt => opt.MapFrom(src => src.ExchangeAccount.Name ?? "Unknown"))
            .ForMember(dest => dest.Exchange, opt => opt.MapFrom(src => src.ExchangeAccount == null || src.ExchangeAccount.Exchange == null ? "Unknown" : src.ExchangeAccount.Exchange.ExchangeName))
            .ForMember(dest => dest.BaseAssetSymbol, opt => opt.MapFrom(src => src.BaseAsset.Symbol ?? "Unknown"))
            .ForMember(dest => dest.BaseAssetName, opt => opt.MapFrom(src => src.BaseAsset.Name ?? "Unknown"))
            .ForMember(dest => dest.QuoteAssetSymbol, opt => opt.MapFrom(src => src.QuoteAsset.Symbol ?? "Unknown"))
            .ForMember(dest => dest.QuoteAssetName, opt => opt.MapFrom(src => src.QuoteAsset.Name ?? "Unknown"));

        CreateMap<Trade, ReportTradeLogViewModel>()
            .ForMember(dest => dest.FeeCurrencySymbol, opt => opt.MapFrom(src => src.FeeCurrency.Symbol ?? "Undefined"))
            .ForMember(dest => dest.FeeCurrencyName, opt => opt.MapFrom(src => src.FeeCurrency.Name ?? "Undefined"));


    }
}
