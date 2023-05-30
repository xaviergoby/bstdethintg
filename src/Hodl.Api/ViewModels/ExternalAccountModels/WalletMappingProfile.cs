namespace Hodl.Api.ViewModels.ExternalAccountModels;

public class WalletMappingProfile : Profile
{
    public WalletMappingProfile()
    {
        // Wallet
        CreateMap<Wallet, WalletListViewModel>();
        CreateMap<Wallet, WalletDetailViewModel>();
        CreateMap<WalletEditViewModel, Wallet>();
        CreateMap<PagingModel<Wallet>, PagingViewModel<WalletListViewModel>>();

        // WalletBalance
        CreateMap<WalletBalance, WalletBalanceListViewModel>();
        CreateMap<WalletBalance, WalletBalanceDetailViewModel>();
        CreateMap<WalletBalanceAddViewModel, WalletBalance>();
        CreateMap<WalletBalanceEditViewModel, WalletBalance>();
        CreateMap<PagingModel<WalletBalance>, PagingViewModel<WalletBalanceListViewModel>>();

        // BlockchainNetwork
        CreateMap<BlockchainNetwork, BlockchainNetworkListViewModel>();
        CreateMap<BlockchainNetwork, BlockchainNetworkDetailViewModel>();
        CreateMap<BlockchainNetworkEditViewModel, BlockchainNetwork>();
        CreateMap<PagingModel<BlockchainNetwork>, PagingViewModel<BlockchainNetworkListViewModel>>();

        // TokenContract
        CreateMap<TokenContract, TokenContractListViewModel>();
    }
}
