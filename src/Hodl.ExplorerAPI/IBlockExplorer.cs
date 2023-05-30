//using System.Transactions;
//using Hodl.ExplorerAPI.Models;

namespace Hodl.ExplorerAPI;

public interface IBlockExplorer
{
    bool SupportsExplorerUrl(string url);

    bool SupportsAddress(string walletAddress);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="explorerUrl">E.g. "https://bscscan.com" or "https://testnet.bscscan.com" or "https://etherscan.io" etc.... </param>
    /// <param name="walletAddress"></param>
    /// <param name="tokenContracts"></param>
    /// <param name="cancelationToken"></param>
    /// <returns></returns>
    Task<IEnumerable<ExplorerBalance>> GetBalances(string explorerUrl, string walletAddress, IEnumerable<TokenContract> tokenContracts, CancellationToken cancelationToken = default);

    Task<IEnumerable<Transaction>> NormalTransactionsByAddress(string explorerUrl, string walletAddress, string startBlock = "0", string endBlock = "99999999", string page = "1", string offset = "10", string sort = "asc", CancellationToken cancellationToken = default);

    Task<IEnumerable<Transaction>> InternalTransactionsByAddress(string explorerUrl, string walletAddress, string startBlock, string endBlock, string page, string offset, string sort, CancellationToken cancellationToken = default);

    Task<IEnumerable<Transaction>> InternalTransactionsByHash(string explorerUrl, string txhash, CancellationToken cancelationToken = default);
}
