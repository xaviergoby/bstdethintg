namespace Hodl.Api.ViewModels.ExternalAccountModels;

public class BankMappingProfile : Profile
{
    public BankMappingProfile()
    {
        // Bank
        CreateMap<Bank, BankListViewModel>();
        CreateMap<Bank, BankDetailViewModel>();
        CreateMap<BankEditViewModel, Bank>();
        CreateMap<PagingModel<Bank>, PagingViewModel<BankListViewModel>>();

        // BankAccount
        CreateMap<BankAccount, BankAccountListViewModel>();
        CreateMap<BankAccount, BankAccountDetailViewModel>();
        CreateMap<BankAccountEditViewModel, BankAccount>();
        CreateMap<PagingModel<BankAccount>, PagingViewModel<BankAccountListViewModel>>();

        // BankBalance
        CreateMap<BankBalance, BankBalanceListViewModel>();
        CreateMap<BankBalance, BankBalanceDetailViewModel>();
        CreateMap<BankBalanceAddViewModel, BankBalance>();
        CreateMap<BankBalanceEditViewModel, BankBalance>();
        CreateMap<PagingModel<BankBalance>, PagingViewModel<BankBalanceListViewModel>>();
    }
}
