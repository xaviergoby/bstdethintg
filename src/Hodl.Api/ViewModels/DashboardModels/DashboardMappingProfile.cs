using Hodl.Api.ViewModels.ReportModels;

namespace Hodl.Api.ViewModels.DashboardModels;

public class DashboardMappingProfile : Profile
{
    public DashboardMappingProfile()
    {
        CreateMap<Fund, DashboardFundCardView>();
        CreateMap<Holding, DashboardHoldingCardView>()
            .ForMember(dest => dest.CurrencySymbol, opt => opt.MapFrom(src => src.Currency == null ? src.CryptoCurrency.Symbol : src.Currency.ISOCode))
            .ForMember(dest => dest.CurrencyName, opt => opt.MapFrom(src => src.Currency == null ? src.CryptoCurrency.Name : src.Currency.Name))
            .ForMember(dest => dest.IsFiat, opt => opt.MapFrom(src => src.Currency != null || src.CryptoCurrency.IsStableCoin))
            .ForMember(dest => dest.IsLocked, opt => opt.MapFrom(src => src.Currency == null && src.CryptoCurrency.IsLocked));
        CreateMap<Holding, DashboardHoldingTableView>()
            .ForMember(dest => dest.CurrencySymbol, opt => opt.MapFrom(src => src.Currency == null ? src.CryptoCurrency.Symbol : src.Currency.ISOCode))
            .ForMember(dest => dest.CurrencyName, opt => opt.MapFrom(src => src.Currency == null ? src.CryptoCurrency.Name : src.Currency.Name))
            .ForMember(dest => dest.IsFiat, opt => opt.MapFrom(src => src.Currency != null || src.CryptoCurrency.IsStableCoin))
            .ForMember(dest => dest.IsLocked, opt => opt.MapFrom(src => src.Currency == null && src.CryptoCurrency.IsLocked))
            .ForMember(dest => dest.EndUSDValue, opt => opt.MapFrom(src => src.EndBalance * src.EndUSDPrice))
            .ForMember(dest => dest.EndBTCValue, opt => opt.MapFrom(src => src.EndBalance * src.EndBTCPrice));

        CreateMap<FundCategory, DashboardFundCategoryCardView>().IncludeMembers(fc => fc.Category);
        CreateMap<Category, DashboardFundCategoryCardView>();

        CreateMap<Transfer, DashboardTransfersLogModel>()
            .ForMember(dest => dest.CurrencySymbol, opt => opt.MapFrom(src => src.Holding.Currency == null ? src.Holding.CryptoCurrency.Symbol : src.Holding.Currency.ISOCode))
            .ForMember(dest => dest.CurrencyName, opt => opt.MapFrom(src => src.Holding.Currency == null ? src.Holding.CryptoCurrency.Name : src.Holding.Currency.Name))
            .ForMember(dest => dest.FeeCurrencySymbol, opt => opt.MapFrom(src => src.FeeHolding.Currency == null ? src.FeeHolding.CryptoCurrency.Symbol : src.FeeHolding.Currency.ISOCode))
            .ForMember(dest => dest.FeeCurrencyName, opt => opt.MapFrom(src => src.FeeHolding.Currency == null ? src.FeeHolding.CryptoCurrency.Name : src.FeeHolding.Currency.Name));

        CreateMap<ReportTradeSummaryViewModel, DashboardTradeSummaryTableView>();
        CreateMap<Holding, DashboardTradeSummaryTableView>()
            .ForMember(dest => dest.CurrencySymbol, opt => opt.MapFrom(src => src.Currency == null ? src.CryptoCurrency.Symbol : src.Currency.ISOCode))
            .ForMember(dest => dest.CurrencyName, opt => opt.MapFrom(src => src.Currency == null ? src.CryptoCurrency.Name : src.Currency.Name))
            .ForMember(dest => dest.IsFiat, opt => opt.MapFrom(src => src.Currency != null || src.CryptoCurrency.IsStableCoin))
            .ForMember(dest => dest.IsLocked, opt => opt.MapFrom(src => src.Currency == null && src.CryptoCurrency.IsLocked));
        CreateMap<FundLayer, DashboardFundLayerCardView>();
    }
}
