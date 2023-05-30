namespace Hodl.Api.ViewModels.FundModels;

public class FundMappingProfile : Profile
{
    public FundMappingProfile()
    {
        CreateMap<Fund, FundListViewModel>();
        CreateMap<Fund, FundDetailViewModel>();
        CreateMap<PagingModel<Fund>, PagingViewModel<FundListViewModel>>();
        CreateMap<FundEditViewModel, Fund>();

        CreateMap<FundOwner, FundOwnerListViewModel>();
        CreateMap<FundOwner, FundOwnerDetailViewModel>();
        CreateMap<PagingModel<FundOwner>, PagingViewModel<FundOwnerListViewModel>>();
        CreateMap<FundOwnerEditViewModel, FundOwner>();

        CreateMap<FundLayer, FundLayerViewModel>();
        CreateMap<FundLayerEditViewModel, FundLayer>();

        CreateMap<Category, CategoryViewModel>();
        CreateMap<PagingModel<Category>, PagingViewModel<CategoryViewModel>>();
        CreateMap<CategoryEditViewModel, Category>();

        CreateMap<FundCategory, FundCategoryViewModel>().IncludeMembers(fc => fc.Category);
        CreateMap<Category, FundCategoryViewModel>();
        CreateMap<FundCategoryEditViewModel, FundCategory>();

        CreateMap<Holding, HoldingListViewModel>()
            .ForMember(dest => dest.EndUSDValue, opt => opt.MapFrom(src => src.EndBalance * src.EndUSDPrice))
            .ForMember(dest => dest.EndBTCValue, opt => opt.MapFrom(src => src.EndBalance * src.EndBTCPrice));
        CreateMap<Holding, HoldingDetailViewModel>();
        CreateMap<HoldingAddViewModel, Holding>();
        CreateMap<HoldingEditViewModel, Holding>();

        CreateMap<Nav, DailyNavViewModel>();
        CreateMap<DailyNavEditViewModel, Nav>();
        CreateMap<Nav, PeriodNavViewModel>();
        CreateMap<PeriodNavEditViewModel, Nav>();
    }
}
