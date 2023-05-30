namespace Hodl.Api.ViewModels.CurrencyModels;

public class CurrencyMappingProfile : Profile
{
    public CurrencyMappingProfile()
    {
        // Currency
        CreateMap<Currency, CurrencyListViewModel>();
        CreateMap<Currency, CurrencyDetailViewModel>();
        CreateMap<CurrencyEditViewModel, Currency>();
        CreateMap<PagingModel<Currency>, PagingViewModel<CurrencyListViewModel>>();

        // CurrencyRate
        CreateMap<CurrencyRate, CurrencyRateViewModel>();
        CreateMap<CurrencyRateEditViewModel, CurrencyRate>();
        CreateMap<PagingModel<CurrencyRate>, PagingViewModel<CurrencyRateViewModel>>();

        // CryptoCurrency
        CreateMap<CryptoCurrency, CryptoCurrencyListViewModel>();
        CreateMap<CryptoCurrency, CryptoCurrencyDetailViewModel>();
        CreateMap<CryptoCurrencyEditViewModel, CryptoCurrency>()
            .ForMember(m => m.Icon, m => m.MapFrom(vm => ImageHelper.ConvertToPng(ImageHelper.ImageDataFromBase64(vm.Icon), new SixLabors.ImageSharp.Size(64, 64))));
        CreateMap<PagingModel<CryptoCurrency>, PagingViewModel<CryptoCurrencyListViewModel>>();

        // Listing
        CreateMap<Listing, ListingViewModel>();
        CreateMap<ListingEditViewModel, Listing>();
        CreateMap<PagingModel<Listing>, PagingViewModel<ListingViewModel>>();
    }
}
