namespace CoinGeckoAPI.Interfaces;

public interface ICoinGeckoClient
{
    ICoinsClient CoinsClient { get; }
    IExchangeRatesClient ExchangeRatesClient { get; }
    IExchangesClient ExchangesClient { get; }
    IGlobalClient GlobalClient { get; }
    ISimpleClient SimpleClient { get; }
    IPingClient PingClient { get; }
    IContractClient ContractClient { get; }
    IFinancePlatformsClient FinancePlatformsClient { get; }
    IIndexesClient IndexesClient { get; }
    IDerivativesClient DerivativesClient { get; }
    ISearchClient SearchClient { get; }
}