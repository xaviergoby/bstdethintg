using Hodl.Api.FilterParams;
using Hodl.Api.HodlDbDomain;
using Hodl.Api.ViewModels.ExternalAccountModels;
using Hodl.Api.ViewModels.TransactionModels;
using Hodl.ExplorerAPI;
using Hodl.ExplorerAPI.Models;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;

namespace Hodl.Api.Controllers.ExternalAccounts;

[ApiController]
[Route("wallets")]
public class WalletController : BaseController
{
    private readonly HodlDbContext _db;
    private readonly ICryptoCurrencyService _cryptoCurrencyService;
    private readonly IChangeLogService _changeLogService;
    private readonly IWalletService _walletService;

    private readonly bool _isTestEnvironment = true;

    public WalletController(
        IOptions<AppDefaults> settings,
        HodlDbContext dbContext,
        ICryptoCurrencyService cryptoCurrencyService,
        IChangeLogService changeLogService,
        IWalletService walletService,
        IMapper mapper,
        ILogger<WalletController> logger,
        IErrorManager errorManager)
        : base(mapper, logger, errorManager)
    {
        _db = dbContext;
        _cryptoCurrencyService = cryptoCurrencyService;
        _changeLogService = changeLogService;
        _walletService = walletService;

        _isTestEnvironment = settings.Value.IsTestEnvironment();
    }

    #region Wallets

    /// <summary>
    /// Gets a list of all wallets, optionally filtered by fund or exchange accoutn.
    /// </summary>
    /// <param name="fundId"></param>
    /// <param name="exchangeAccountId"></param>
    /// <param name="page"></param>
    /// <param name="itemsPerPage"></param>
    /// <returns></returns>
    [HttpGet]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<ActionResult<PagingViewModel<WalletListViewModel>>> Get(Guid? fundId, Guid? exchangeAccountId, int page, int? itemsPerPage, CancellationToken ct)
    {
        // you can skip properies here
        var filter = new WalletFilterParams
        {
            FundId = fundId,
            ExchangeAccountId = exchangeAccountId
        };

        var query = _db.Wallets
            .AsNoTracking()
            .Filter(filter)
            .Include(w => w.ExchangeAccount)
            .ThenInclude(a => a.Exchange)
            .OrderByDescending(w => w.Timestamp)
            .AsSingleQuery();

        // Create a paged resultset
        var pageResult = await query.PaginateAsync(page, itemsPerPage ?? PagingConstants.DEFAULT_ITEMS_PER_PAGE, ct);
        var pageResultView = _mapper.Map<PagingViewModel<WalletListViewModel>>(pageResult);

        return Ok(pageResultView);
    }

    /// <summary>
    /// Gets the wallet for the given address.
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("{address}")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<ActionResult<WalletDetailViewModel>> GetWallet(string address, CancellationToken ct)
    {
        var wallet = await _db.Wallets
            .AsNoTracking()
            .Where(w => w.Address == address)
            .Include(w => w.WalletBalances.OrderByDescending(b => b.Timestamp))
            .ThenInclude(b => b.BlockchainNetwork)
            .Include(w => w.ExchangeAccount)
            .ThenInclude(a => a.Exchange)
            .AsSplitQuery()
            .SingleOrDefaultAsync(ct);

        if (wallet == null)
            return NotFound();

        return Ok(_mapper.Map<WalletDetailViewModel>(wallet));
    }

    /// <summary>
    /// Add a new Wallet.
    /// </summary>
    /// <param name="wallet"></param>
    /// <returns></returns>
    /// <exception cref="RestException"></exception>
    [HttpPost]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<ActionResult<WalletListViewModel>> PostWallet([FromBody] WalletEditViewModel wallet)
    {
        if (!ModelState.IsValid)
            return BadRequest("Invalid data.");

        // TODO: Add Wallet address validator for all types of wallets

        var newWallet = _mapper.Map<Wallet>(wallet);

        using IDbContextTransaction transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            await _changeLogService.AddChangeLogAsync("Wallets", null, newWallet);
            await _db.Wallets.AddAsync(newWallet);
            await _db.SaveChangesAsync();

            transaction.Commit();
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            throw new RestException(HttpStatusCode.InternalServerError, new ErrorInformationItem
            {
                Code = ErrorCodesStore.InternalServerError,
                Description = ex.Message
            });
        }

        return Ok(_mapper.Map<WalletListViewModel>(newWallet));
    }

    /// <summary>
    /// Modify an existing Wallet.
    /// </summary>
    /// <param name="address"></param>
    /// <param name="wallet"></param>
    /// <returns></returns>
    [HttpPut]
    [Route("{address}")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<IActionResult> PutWallet(string address, [FromBody] WalletEditViewModel wallet)
    {
        if (!ModelState.IsValid)
            return BadRequest("Invalid data.");

        var storedWallet = await _db.Wallets
            .Where(w => w.Address == address)
            .SingleOrDefaultAsync();

        if (storedWallet == null)
            return NotFound();

        // TODO: Check Wallet address format!

        using IDbContextTransaction transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            await _changeLogService.AddChangeLogAsync("Wallets", storedWallet, wallet);
            _mapper.Map(wallet, storedWallet);
            await _db.SaveChangesAsync();

            transaction.Commit();
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            throw new RestException(HttpStatusCode.InternalServerError, new ErrorInformationItem
            {
                Code = ErrorCodesStore.InternalServerError,
                Description = ex.Message
            });
        }

        return Ok();
    }

    /// <summary>
    /// Remove Exchange.
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    [HttpDelete]
    [Route("{address}")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<IActionResult> DeleteWallet(string address)
    {
        var storedWallet = await _db.Wallets
            .Where(w => w.Address == address)
            .Include(w => w.WalletBalances)
            .AsSplitQuery()
            .SingleOrDefaultAsync();

        if (storedWallet == null)
            return NotFound();

        // Do checks for existing accounts.
        if (storedWallet.WalletBalances.Any())
            _errorManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
            {
                Field = nameof(Wallet.WalletBalances),
                Code = ErrorCodesStore.ItemIsReferenced,
                Description = "Wallet has balances linked so can not be deleted."
            });

        _errorManager.ThrowOnErrors();

        using IDbContextTransaction transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            await _changeLogService.AddChangeLogAsync("Wallets", storedWallet, null);
            _db.Wallets.Remove(storedWallet);
            await _db.SaveChangesAsync();

            transaction.Commit();
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            throw new RestException(HttpStatusCode.InternalServerError, new ErrorInformationItem
            {
                Code = ErrorCodesStore.InternalServerError,
                Description = ex.Message
            });
        }

        return Ok();
    }

    #endregion

    #region WalletBalances

    /// <summary>
    /// Gets a list of all balances in a wallet.
    /// </summary>
    /// <param name="address"></param>
    /// <param name="page"></param>
    /// <param name="itemsPerPage"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("{address}/balances")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<ActionResult<PagingViewModel<WalletBalanceListViewModel>>> GetWalletBalances(string address, int page, int? itemsPerPage, CancellationToken ct)
    {
        var query = _db.WalletBalances
            .AsNoTracking()
            .Where(b => b.Address == address)
            .Include(b => b.Wallet)
            .Include(b => b.CryptoCurrency)
            .Include(b => b.BlockchainNetwork)
            .OrderByDescending(w => w.Timestamp)
            .AsSplitQuery();

        // Create a paged resultset
        var pageResult = await query.PaginateAsync(page, itemsPerPage ?? PagingConstants.DEFAULT_ITEMS_PER_PAGE, ct);
        var pageResultView = _mapper.Map<PagingViewModel<WalletBalanceListViewModel>>(pageResult);

        return Ok(pageResultView);
    }

    /// <summary>
    /// Gets the wallet for the given address.
    /// </summary>
    /// <param name="address"></param>
    /// <param name="balanceId"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("{address}/balances/{balanceId}")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<ActionResult<WalletBalanceDetailViewModel>> GetWalletBalance(string address, Guid balanceId, CancellationToken ct)
    {
        var balance = await _db.WalletBalances
            .AsNoTracking()
            .Where(b => b.Address == address && b.Id == balanceId)
            .Include(b => b.Wallet)
            .Include(b => b.CryptoCurrency)
            .Include(b => b.BlockchainNetwork)
            .AsSplitQuery()
            .SingleOrDefaultAsync(ct);

        if (balance == null)
            return NotFound();

        return Ok(_mapper.Map<WalletBalanceDetailViewModel>(balance));
    }

    /// <summary>
    /// Add a new Wallet balance.
    /// </summary>
    /// <param name="balance"></param>
    /// <returns></returns>
    /// <exception cref="RestException"></exception>
    [HttpPost]
    [Route("{address}/balances")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<ActionResult<WalletBalanceListViewModel>> PostWalletBalance([FromBody] WalletBalanceAddViewModel balance)
    {
        if (!ModelState.IsValid)
            return BadRequest("Invalid data.");

        // Check references
        if (await _db.Wallets.SingleOrDefaultAsync(w => w.Address == balance.Address) == null)
            _errorManager.AddValidationError(HttpStatusCode.NotFound, new ErrorInformationItem
            {
                Field = nameof(WalletBalance.Wallet),
                Code = ErrorCodesStore.ReferencedRecordNotFound,
                Description = "Referenced wallet not found."
            });
        if (await _db.BlockchainNetworks.SingleOrDefaultAsync(n => n.Id == balance.BlockchainNetworkId) == null)
            _errorManager.AddValidationError(HttpStatusCode.NotFound, new ErrorInformationItem
            {
                Field = nameof(WalletBalance.BlockchainNetworkId),
                Code = ErrorCodesStore.ReferencedRecordNotFound,
                Description = "Referenced blockchain network not found."
            });
        // Check unique balance currency
        if (await _db.WalletBalances.AnyAsync(b => b.Address == balance.Address
            && b.BlockchainNetworkId == balance.BlockchainNetworkId
            && b.CryptoId == balance.CryptoId))
            _errorManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
            {
                Field = "Address + BlockchainNetwork + CryptoCurrency",
                Code = ErrorCodesStore.DuplicateKey,
                Description = "The balance for the crypto currency in this wallet already exists."
            });

        _errorManager.ThrowOnErrors();

        var newBalance = _mapper.Map<WalletBalance>(balance);

        using IDbContextTransaction transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            await _changeLogService.AddChangeLogAsync("WalletBalances", null, newBalance);
            await _db.WalletBalances.AddAsync(newBalance);
            await _db.SaveChangesAsync();

            transaction.Commit();
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            throw new RestException(HttpStatusCode.InternalServerError, new ErrorInformationItem
            {
                Code = ErrorCodesStore.InternalServerError,
                Description = ex.Message
            });
        }

        return Ok(_mapper.Map<WalletBalanceListViewModel>(newBalance));
    }

    /// <summary>
    /// Modify an existing Wallet.
    /// </summary>
    /// <param name="address"></param>
    /// <param name="balanceId"></param>
    /// <param name="balance"></param>
    /// <returns></returns>
    [HttpPut]
    [Route("{address}/balances/{balanceId}")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<IActionResult> PutWalletBalance(string address, Guid balanceId, [FromBody] WalletBalanceEditViewModel balance)
    {
        if (!ModelState.IsValid)
            return BadRequest("Invalid data.");

        var storedBalance = await _db.WalletBalances
            .Where(b => b.Id == balanceId)
            .SingleOrDefaultAsync();

        if (storedBalance == null)
            return NotFound();

        if (storedBalance.Address != address)
            _errorManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
            {
                Field = nameof(Listing.CryptoId),
                Code = ErrorCodesStore.ReferenceCheckFailed,
                Description = "The wallet address for the balance does not match."
            });

        // Check references
        if (await _db.Wallets.SingleOrDefaultAsync(w => w.Address == address) == null)
            _errorManager.AddValidationError(HttpStatusCode.NotFound, new ErrorInformationItem
            {
                Field = nameof(WalletBalance.Wallet),
                Code = ErrorCodesStore.ReferencedRecordNotFound,
                Description = "Referenced wallet not found."
            });

        _errorManager.ThrowOnErrors();

        using IDbContextTransaction transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            await _changeLogService.AddChangeLogAsync("WalletBalances", storedBalance, balance);
            _mapper.Map(balance, storedBalance);
            await _db.SaveChangesAsync();

            transaction.Commit();
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            throw new RestException(HttpStatusCode.InternalServerError, new ErrorInformationItem
            {
                Code = ErrorCodesStore.InternalServerError,
                Description = ex.Message
            });
        }

        return Ok();
    }

    /// <summary>
    /// Remove Exchange.
    /// </summary>
    /// <param name="address"></param>
    /// <param name="balanceId"></param>
    /// <returns></returns>
    [HttpDelete]
    [Route("{address}/balances/{balanceId}")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<IActionResult> DeleteExchange(string address, Guid balanceId)
    {
        var storedBalance = await _db.WalletBalances
            .Where(b => b.Id == balanceId)
            .SingleOrDefaultAsync();

        if (storedBalance == null)
            return NotFound();

        if (storedBalance.Address != address)
            _errorManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
            {
                Field = nameof(BankBalance.BankAccountId),
                Code = ErrorCodesStore.ReferenceCheckFailed,
                Description = "The bank account for the balance does not match."
            });

        _errorManager.ThrowOnErrors();

        using IDbContextTransaction transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            await _changeLogService.AddChangeLogAsync("WalletBalances", storedBalance, null);
            _db.WalletBalances.Remove(storedBalance);
            await _db.SaveChangesAsync();

            transaction.Commit();
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            throw new RestException(HttpStatusCode.InternalServerError, new ErrorInformationItem
            {
                Code = ErrorCodesStore.InternalServerError,
                Description = ex.Message
            });
        }

        return Ok();
    }

    #endregion

    #region Networks

    /// <summary>
    /// Gets a paged list of all blockchain networks.
    /// </summary>
    /// <param name="page"></param>
    /// <param name="itemsPerPage"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("/networks")]
    public async Task<ActionResult<PagingViewModel<BlockchainNetworkListViewModel>>> GetBlockchainNetworks(int page, int? itemsPerPage, CancellationToken ct)
    {
        var query = _db.BlockchainNetworks
            .AsNoTracking()
            .OrderBy(ea => ea.Name);

        // Create a paged resultset
        var pageResult = await query.PaginateAsync(page, itemsPerPage ?? PagingConstants.DEFAULT_ITEMS_PER_PAGE, ct);

        var pageResultView = _mapper.Map<PagingViewModel<BlockchainNetworkListViewModel>>(pageResult);

        return Ok(pageResultView);
    }

    /// <summary>
    /// Gets the selected ExchangeAccount.
    /// </summary>
    /// <param name="blockchainNetworkId"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("/networks/{blockchainNetworkId}")]
    public async Task<ActionResult<BlockchainNetworkDetailViewModel>> GetBlockchainNetwork(Guid blockchainNetworkId, CancellationToken ct)
    {
        var blockchainNetwork = await _db.BlockchainNetworks
            .AsNoTracking()
            .Where(n => n.Id == blockchainNetworkId)
            .Include(n => n.WalletBalances)
            .AsSplitQuery()
            .SingleOrDefaultAsync(ct);

        if (blockchainNetwork == null)
            return NotFound();

        return Ok(_mapper.Map<BlockchainNetworkDetailViewModel>(blockchainNetwork));
    }

    /// <summary>
    /// Add a new exchangeAccount.
    /// </summary>
    /// <param name="blockchainNetwork"></param>
    /// <returns></returns>
    /// <exception cref="RestException"></exception>
    [HttpPost]
    [Route("/networks")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<ActionResult<BlockchainNetworkListViewModel>> PostBlockchainNetwork([FromBody] BlockchainNetworkEditViewModel blockchainNetwork)
    {
        if (!ModelState.IsValid)
            return BadRequest("Invalid data.");

        var newBlockchainNetwork = new BlockchainNetwork();

        _mapper.Map(blockchainNetwork, newBlockchainNetwork);

        await _db.BlockchainNetworks.AddAsync(newBlockchainNetwork);
        await _db.SaveChangesAsync();

        return Ok(_mapper.Map<BlockchainNetworkListViewModel>(newBlockchainNetwork));
    }

    /// <summary>
    /// Update a rating.
    /// </summary>
    /// <param name="blockchainNetworkId"></param>
    /// <param name="id"></param>
    /// <param name="blockchainNetwork"></param>
    /// <returns></returns>
    [HttpPut]
    [Route("/networks/{blockchainNetworkId}")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<IActionResult> PutBlockchainNetwork(Guid blockchainNetworkId, [FromBody] BlockchainNetworkEditViewModel blockchainNetwork)
    {
        if (!ModelState.IsValid)
            return BadRequest("Invalid data.");

        var storedBlockchainNetwork = await _db.BlockchainNetworks
            .Where(n => n.Id == blockchainNetworkId)
            .SingleOrDefaultAsync();

        if (storedBlockchainNetwork == null)
            return NotFound();

        _mapper.Map(blockchainNetwork, storedBlockchainNetwork);
        await _db.SaveChangesAsync();

        return Ok();
    }

    /// <summary>
    /// Removes the Exchange rating.
    /// </summary>
    /// <param name="blockchainNetworkId"></param>
    /// <returns></returns>
    [HttpDelete]
    [Route("/networks/{blockchainNetworkId}")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<IActionResult> DeleteBlockchainNetwork(Guid blockchainNetworkId)
    {
        var storedBlockchainNetwork = await _db.BlockchainNetworks
            .Where(n => n.Id == blockchainNetworkId)
            .SingleOrDefaultAsync();

        if (storedBlockchainNetwork == null)
            return NotFound();

        // Do checks for existing balances and return nice error messages.
        if (await _db.WalletBalances.AnyAsync(t => t.BlockchainNetworkId == blockchainNetworkId))
            _errorManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
            {
                Field = nameof(BlockchainNetwork.WalletBalances),
                Code = ErrorCodesStore.ItemIsReferenced,
                Description = "Blockchain network has balances linked so can not be deleted."
            });

        _errorManager.ThrowOnErrors();

        _db.BlockchainNetworks.Remove(storedBlockchainNetwork);
        await _db.SaveChangesAsync();

        return Ok();
    }

    #endregion

    #region TokenContracts
    [HttpGet]
    [Route("/tokencontracts/import/{cryptoId}")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<ActionResult<IEnumerable<TokenContractListViewModel>>> ImportTokenContracts(Guid cryptoId)
    {
        var crypto = await _cryptoCurrencyService.GetCryptoCurrencyAsync(cryptoId) ??
            throw new NotFoundException("Cryptocurrency not found");

        var tokenContracts = await _cryptoCurrencyService.ImportTokenContracts(crypto);

        return Ok(tokenContracts.Select(c => _mapper.Map<TokenContractListViewModel>(c)));
    }
    #endregion

    #region Explorers

    /// <summary>
    /// Gets a list of all balances in a wallet.
    /// </summary>
    /// <param name="address"></param>
    /// <param name="page"></param>
    /// <param name="itemsPerPage"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("explorer/{address}/balances")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<ActionResult<IEnumerable<ExplorerBalance>>> GetExplorerWalletBalances(string address, string explorerUrl, CancellationToken ct)
    {
        var explorers = HttpContext.RequestServices.GetServices<IBlockExplorer>()
            .Where(e => e.SupportsAddress(address) && e.SupportsExplorerUrl(explorerUrl));
        var blockChains = await _db.BlockchainNetworks
            .AsNoTracking()
            .Where(c => c.ExplorerUrl.Equals(explorerUrl))
            .Include(n => n.TokenContracts)
            .ThenInclude(c => c.CryptoCurrency)
            .AsSingleQuery()
            .ToArrayAsync(ct);
        var balances = new List<ExplorerBalance>();

        foreach (var blockchain in blockChains)
        {
            foreach (var explorer in explorers)
            {
                try
                {
                    if (explorer.SupportsAddress(address) && explorer.SupportsExplorerUrl(blockchain.ExplorerUrl))
                    {
                        var tokens = blockchain.TokenContracts.Select(c => new ExplorerAPI.Models.TokenContract()
                        {
                            Address = c.ContractAddress,
                            TokenName = c.CryptoCurrency.Name,
                            TokenSymbol = c.CryptoCurrency.Symbol,
                            Decimals = c.CryptoCurrency.Decimals
                        });
                        var explorerBalances = await explorer.GetBalances(blockchain.ExplorerUrl, address, tokens, ct);

                        balances.AddRange(explorerBalances.Where(b => b.Balance > 0));
                    }
                }
                catch (NotImplementedException) { }
            }
        }

        return Ok(balances);
    }

    #endregion

    /// <summary>
    /// This method is for manually importing the balances of a given wallet (address).
    /// </summary>
    /// <param name="address">E.g. SwissBorg Ethereum Account #3: 0x87cbc48075d7aa1760Ac71C41e8Bc289b6A31F56</param>
    /// <returns>The number of (new) wallet balances imported</returns>
    [HttpGet]
    [Route("balances/{address}/import")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<IActionResult> ImportWalletBalances(string address)
    {
        var wallet = await _db.Wallets
            .Where(w => w.Address.Equals(address))
            .FirstOrDefaultAsync();

        if (wallet == null)
            return NotFound($"Wallet {address} not found.");

        return await _walletService.ImportWalletBalances(wallet, _isTestEnvironment)
            ? Ok()
            : Problem(detail: $"Failed to import balances for wallet {address}");
    }

    /// <summary>
    /// Gets a list of all normal transactions associated with an (external account/wallet) address.
    /// </summary>
    /// <param name="address"></param>
    /// <param name="page"></param>
    /// <param name="itemsPerPage"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("explorer/{address}/normalTransactions")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<ActionResult<Transaction>> GetNormalTransactionByAddress(string address, string explorerUrl, CancellationToken cancellationToken)
    {
        var explorer = HttpContext.RequestServices.GetServices<IBlockExplorer>()
            .Where(e => e.SupportsAddress(address) && e.SupportsExplorerUrl(explorerUrl))
            .SingleOrDefault();

        var blockchain = await _db.BlockchainNetworks
            .AsNoTracking()
            .Where(b => explorer.SupportsAddress(address) && explorer.SupportsExplorerUrl(b.ExplorerUrl))
            .Include(b => b.IsTestNet)
            .AsSingleQuery()
            .SingleOrDefaultAsync();


        //if (!_isTestEnvironment && blockchain.IsTestNet) continue;


        var explorerTransactions = await explorer.NormalTransactionsByAddress(blockchain.ExplorerUrl, address, cancellationToken: cancellationToken);

        if (explorerTransactions.Any())
        {
            foreach (var explorerTransaction in explorerTransactions)
            {
                var result = _mapper.Map<TransactionDetailViewModel>(explorerTransaction);
            }

            // Now save all the orders generated from mapping the blockchain transaction data into the order dto
            await _db.Orders.AddRangeAsync(orders, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
        }

        foreach (var blockchain in blockChains)
        {
            // Skip testnets for production environment
            if (!_isTestEnvironment && blockchain.IsTestNet) continue;

            var explorerTransactions = new List<Transaction>();

            foreach (var explorer in explorers
                .Where(e => e.SupportsAddress(address) && e.SupportsExplorerUrl(blockchain.ExplorerUrl)))
                try
                {
                    var explorerResult = await explorer.NormalTransactionsByAddress(blockchain.ExplorerUrl, address);

                    explorerTransactions.AddRange(explorerResult);
                }
                catch (NotImplementedException) { }

            if (explorerTransactions.Any())
            {
                var orders = new List<Order>();

                foreach (var explorerTransaction in explorerTransactions)
                {
                    var order = _mapper.Map<Order>(explorerTransaction);
                    orders.Add(order);
                }

                // Now save all the orders generated from mapping the blockchain transaction data into the order dto
                await _db.Orders.AddRangeAsync(orders, cancellationToken);
                await _db.SaveChangesAsync(cancellationToken);
            }
        }

        return Ok(balances);


        return await _walletService.ImportWalletBalances(wallet, _isTestEnvironment)
            ? Ok()
            : Problem(detail: $"Failed to import balances for wallet {address}");
    }



        /// <summary>
        /// This method is for manually importing the normal transactions associated with an (external) account/wallet for a given address.
        /// </summary>
        /// <param name="address">E.g. SwissBorg Ethereum Account #3: 0x87cbc48075d7aa1760Ac71C41e8Bc289b6A31F56</param>
        /// <returns>The number of (new) wallet balances imported</returns>
        [HttpGet]
    [Route("normalTransactions/{walletAddress}/import")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<IActionResult> ImportWalletNormalTransactions(string walletAddress, CancellationToken cancellationToken)
    {
        var wallet = await _db.Wallets
            .Where(w => w.Address.Equals(walletAddress))
            .FirstOrDefaultAsync();

        if (wallet == null)
            return NotFound($"Wallet {walletAddress} not found in database.");

        return await _walletService.ImportWalletNormalTransactions(wallet, _isTestEnvironment)
            ? Ok()
            : Problem(detail: $"Failed to import the normal transactions for wallet {walletAddress}");
    }
}
