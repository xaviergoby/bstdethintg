namespace Hodl.Api.ViewModels.ExternalAccountModels;

public class ExchangeMappingProfile : Profile
{
    public ExchangeMappingProfile()
    {
        // Exchange
        CreateMap<Exchange, ExchangeListViewModel>();
        CreateMap<Exchange, ExchangeDetailViewModel>();
        CreateMap<ExchangeEditViewModel, Exchange>()
            .ForMember(m => m.Icon, m => m.MapFrom(vm => ImageHelper.ConvertToPng(ImageHelper.ImageDataFromBase64(vm.Icon), new SixLabors.ImageSharp.Size(64, 64))));
        CreateMap<PagingModel<Exchange>, PagingViewModel<ExchangeListViewModel>>();

        // ExchangeAccount
        CreateMap<ExchangeAccount, ExchangeAccountListViewModel>();
        CreateMap<ExchangeAccount, ExchangeAccountDetailViewModel>()
            .ForMember(dest => dest.AccountSecret, opt => opt.MapFrom(src => new string('*', src.AccountSecret.Length)));
        CreateMap<ExchangeAccountEditViewModel, ExchangeAccount>()
            .ForMember(dest => dest.AccountSecret, opt => opt.Condition(src => !string.IsNullOrEmpty(src.AccountSecret) && !string.IsNullOrEmpty(src.AccountSecret.Replace("*", ""))));
        CreateMap<PagingModel<ExchangeAccount>, PagingViewModel<ExchangeAccountListViewModel>>();
    }
}
