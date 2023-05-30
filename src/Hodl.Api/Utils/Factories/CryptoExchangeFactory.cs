using Hodl.ExchangeAPI.Binance;
using Hodl.ExchangeAPI.Interfaces;
using Hodl.ExchangeAPI.Kucoin;

namespace Hodl.Api.Utils.Factories;

public static class CryptoExchangeFactory
{
    [Serializable]
    public class ExchangeApiNotFoundException : Exception
    {
        public ExchangeApiNotFoundException() { }

        public ExchangeApiNotFoundException(string message)
            : base(message) { }

        public ExchangeApiNotFoundException(string message, Exception inner)
            : base(message, inner) { }
    }

    public static IBaseExchangeAPI GetExchangeClient(Exchange exchange, ExchangeAccount account, bool testEnvironment)
    {
        if (exchange is null)
            throw new ArgumentNullException(nameof(exchange));

        // Make sure the GUID's are in the database with the associated
        // exchanges. To do so, add them in the DbContext.
        return exchange.Id.ToString() switch
        {
            "253dfc2d-ac8b-413b-9a8b-095a1a7b621b" => new ExchangeBinance(account.Id, account.AccountKey, account.AccountSecret, testEnvironment),
            "75ef3d4e-236e-413e-931d-3b2cfa69e4e8" => new ExchangeKucoin(account.Id, account.AccountKey, account.AccountSecret, account.AccountPrivateKey, testEnvironment),
            _ => throw new ExchangeApiNotFoundException($"No exchange client found for {exchange.ExchangeName}.")
        };
    }
}
