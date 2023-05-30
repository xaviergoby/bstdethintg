using Hodl.Api.Utils.Factories;
using Hodl.Api.ViewModels.TradingModels;
using Microsoft.Extensions.Options;
using ExplorerAPI.Utils;
using Hodl.Crypto;
using Hodl.ExplorerAPI.Configurations;
using Hodl.Framework;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;
using Hodl.ExplorerAPI.Implementations;
using Hodl.ExplorerAPI;
using Hodl.Api.HodlDbDomain;

namespace Hodl.Api.Controllers.Trading;

[ApiController]
[Route("transactions")]
public class BlockTransactionsController : BaseController
{

    private readonly HodlDbContext _db;
    private readonly IFundService _fundService;
    private readonly IChangeLogService _changeLogService;
    //private readonly EtherScan _etherScan;
    private readonly IServiceProvider _serviceProvider;

    public BlockTransactionsController(
        HodlDbContext dbContext,
        IFundService fundService,
        IChangeLogService changeLogService,
        //EtherScan etherScan,
        IServiceProvider serviceProvider,
        IMapper mapper,
        ILogger<BlockTransactionsController> logger,
        IErrorManager errorManager)
        : base(mapper, logger, errorManager)
    {
        _db = dbContext;
        _fundService = fundService;
        _changeLogService = changeLogService;
        //_etherScan = etherScan;
        _serviceProvider = serviceProvider;
    }

    [HttpGet]
    [Route("{transactionHash}")]
    public async Task<IActionResult> GetTransactionInformation(string transactionHash, CancellationToken cancellationToken)
    {
        //var explorers = _serviceProvider.GetServices<EtherScan>();
        //var explorer = _serviceProvider.GetService<EtherScan>();
        var explorers = _serviceProvider.GetServices<IBlockExplorer>();

        //var explorer = explorers.Where(e => e.SupportsExplorerUrl("https://etherscan.io")).Single();
        var explorer = explorers.Where(e => e.SupportsExplorerUrl("https://etherscan.io")).Single();

        //var result = await explorer.GetTransactionInformation(transactionHash, "https://etherscan.io", cancellationToken);
        var result = await explorer.GetTransactionInformation("https://etherscan.io", transactionHash, cancellationToken);

        //foreach (var explorer in explorers.Where(e => e.SupportsExplorerUrl("https://etherscan.io")))
        //{

        //}

        //var result = await explorer.GetTransactionInformation(transactionHash, "https://etherscan.io", cancellationToken);



        return Ok(result);
    }
}
