using System.Text.RegularExpressions;

namespace Hodl.Crypto;

/// <summary>
/// General crypto functions, like wallet address checks.
/// </summary>
public static class WalletFormat
{


    private const string REG_BTC_WALLET = @"^[13][a-km-zA-HJ-NP-Z1-9]{25,34}$";
    private const string REG_BECH32_WALLET = @"^bc(0([ac-hj-np-z02-9]{39}|[ac-hj-np-z02-9]{59})|1[ac-hj-np-z02-9]{8,87})$";
    private const string REG_ETH_WALLET = @"^0x[a-fA-F0-9]{40}$";
    private const string REG_DASH_WALLET = @"^X[1-9A-HJ-NP-Za-km-z]{33}$";
    private const string REG_XMR_WALLET = @"^[48][0-9AB][1-9A-HJ-NP-Za-km-z]{93}$";
    private const string REG_BCH_WALLET = @"^[13][a-km-zA-HJ-NP-Z1-9]{25,34}$|^(bitcoincash:)?[qp][a-km-zA-HJ-NP-Z1-9]{41}$";
    private const string REG_LTC_WALLET = @"^[LM3][a-km-zA-HJ-NP-Z1-9]{25,34}$";
    private const string REG_BNB_WALLET = @"^(bnb1)[0-9a-z]{38}$";
    private const string REG_AVAX_WALLET = @"^([XPC]|[a-km-zA-HJ-NP-Z1-9]{36,72})-[a-zA-Z]{1,83}1[qpzry9x8gf2tvdw0s3jn54khce6mua7l]{38}$";
    private const string REG_SOL_WALLET = @"^[1-9A-HJ-NP-Za-km-z]{32,44}$";
    private const string REG_NEAR_WALLET = @"^([0-9a-f]{64}|(\w|(?<!\.)\.)+(?<!\.)\.(testnet|near))$";
    private const string REG_DOT_WALLET = @"^[15CDFGHJ][0-9a-zA-Z]{47}$";
    private const string REG_ADA_WALLET = @"^addr1[a-z0-9]+$";


    /* TODO:
     * v Bitcoin - https://btcscan.org/
     * v Ethereum - https://etherscan.io/
     * v Dash
     * v Monero
     * v Bitcoin Cash
     * v Litecoin
     * v BNB - https://bscscan.com/
     * v Avalanche - https://snowtrace.io/
     * v Solana
     * v Arbitrum - https://arbiscan.io/ = ETH
     * v NEAR - https://nearscan.org/home
     * v Polygon - https://polygonscan.com/
     * v Fantom - https://ftmscan.com/
     * v Andromeda - https://andromeda-explorer.metis.io/
     * v Aleph Zero - https://azero.dev/#/explorer
     * v Cardano
     * Algorand
     */

    public static bool IsBitcoinAddress(string address) => Regex.IsMatch(address, REG_BTC_WALLET) || Regex.IsMatch(address, REG_BECH32_WALLET);

    public static bool IsEthereumAddress(string address) => Regex.IsMatch(address, REG_ETH_WALLET);

    public static bool IsDashAddress(string address) => Regex.IsMatch(address, REG_DASH_WALLET);

    public static bool IsMoneroAddress(string address) => Regex.IsMatch(address, REG_XMR_WALLET);

    public static bool IsBitcoinCashAddress(string address) => Regex.IsMatch(address, REG_BCH_WALLET);

    public static bool IsLitecoinAddress(string address) => Regex.IsMatch(address, REG_LTC_WALLET);

    public static bool IsBnbBep20Address(string address) => Regex.IsMatch(address, REG_BNB_WALLET) || IsEthereumAddress(address);

    public static bool IsAvaxAddress(string address) => Regex.IsMatch(address, REG_AVAX_WALLET) || IsEthereumAddress(address);

    public static bool IsSolanaAddress(string address) => Regex.IsMatch(address, REG_SOL_WALLET);

    public static bool IsArbitrumAddress(string address) => IsEthereumAddress(address);

    public static bool IsNearAddress(string address) => Regex.IsMatch(address, REG_NEAR_WALLET);

    public static bool IsPolygonAddress(string address) => IsEthereumAddress(address);

    public static bool IsFantomAddress(string address) => IsEthereumAddress(address);

    public static bool IsMetisAddress(string address) => IsEthereumAddress(address);

    public static bool IsPolkadotAddress(string address) => Regex.IsMatch(address, REG_DOT_WALLET);

    public static bool IsAlephZeroAddress(string address) => IsPolkadotAddress(address);

    public static bool IsCardanoAddress(string address) => Regex.IsMatch(address, REG_ADA_WALLET);
}
