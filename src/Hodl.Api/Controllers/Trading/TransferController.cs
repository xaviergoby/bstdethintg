using Hodl.Api.FilterParams;
using Hodl.Api.ViewModels.TradingModels;
using Microsoft.EntityFrameworkCore.Storage;

namespace Hodl.Api.Controllers.Trading;

[ApiController]
[Route("transfers")]
public class TransferController : BaseController
{
    private static readonly TransactionType[] directionInTransferTypes = new[] { TransactionType.Inflow, TransactionType.Reward, TransactionType.Profit, TransactionType.Correction };
    private static readonly TransactionType[] bidirectionalTransferTypes = new[] { TransactionType.Wallet, TransactionType.Broker, TransactionType.Transfer, TransactionType.Bank };
    private static readonly TransactionType[] exchangeTransferTypes = new[] { TransactionType.Broker, TransactionType.Transfer };


    private readonly HodlDbContext _db;
    private readonly IFundService _fundService;
    private readonly IChangeLogService _changeLogService;

    public TransferController(
        HodlDbContext dbContext,
        IFundService fundService,
        IChangeLogService changeLogService,
        IMapper mapper,
        ILogger<TransferController> logger,
        IErrorManager errorManager)
        : base(mapper, logger, errorManager)
    {
        _db = dbContext;
        _fundService = fundService;
        _changeLogService = changeLogService;
    }

    #region Transfers

    /// <summary>
    /// Gets a paged list of transfers with overview properties.
    /// </summary>
    /// <param name="page">Page index (optional)</param>
    /// <param name="itemsPerPage">Number of items per page (optional, default 20)</param>
    /// <returns></returns>
    [HttpGet]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<ActionResult<PagingViewModel<TransferListViewModel>>> Get(
        Guid? holdingId,
        TransactionType? transactionType,
        string transactionSource,
        TransferDirection? direction,
        string fromBookingPeriod,
        string toBookingPeriod,
        int page, int? itemsPerPage,
        CancellationToken ct)
    {
        // Filters
        var filter = new TransferFilterParams
        {
            HoldingId = holdingId,
            TransactionType = transactionType,
            TransactionSource = transactionSource,
            Direction = direction,
            FromBookingPeriod = fromBookingPeriod,
            ToBookingPeriod = toBookingPeriod
        };

        var query = _db.Transfers
            .AsNoTracking()
            .Filter(filter)
            .Include(t => t.Holding)
            .ThenInclude(h => h.Currency)
            .Include(t => t.Holding)
            .ThenInclude(h => h.CryptoCurrency)
            .OrderByDescending(t => t.DateTime)
            .ThenBy(t => t.Holding.Currency.Symbol)
            .ThenBy(t => t.Holding.CryptoCurrency.Symbol)
            .ThenBy(t => t.Direction)
            .AsSingleQuery();

        // Create a paged resultset
        var pageResult = await query.PaginateAsync(page, itemsPerPage ?? PagingConstants.DEFAULT_ITEMS_PER_PAGE, ct);
        var pageResultView = _mapper.Map<PagingViewModel<TransferListViewModel>>(pageResult);

        return Ok(pageResultView);
    }

    /// <summary>
    /// Gets a paged list of transfers with overview properties.
    /// </summary>
    /// <param name="page">Page index (optional)</param>
    /// <param name="itemsPerPage">Number of items per page (optional, default 20)</param>
    /// <returns></returns>
    [HttpGet]
    [Route("/funds/{fundId}/transfers/")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<ActionResult<PagingViewModel<TransferListViewModel>>> Get(
        Guid fundId,
        string bookingPeriod,
        int page, int? itemsPerPage,
        CancellationToken ct)
    {
        bookingPeriod = await _fundService.GetValidBookingPeriod(bookingPeriod, fundId, ct);

        var holdingIds = await _db.Holdings
            .AsNoTracking()
            .Where(h => h.FundId == fundId && h.BookingPeriod == bookingPeriod)
            .Select(h => h.Id)
            .ToListAsync(ct);

        var query = _db.Transfers
            .AsNoTracking()
            .Where(t => holdingIds.Contains(t.HoldingId))
            .Include(t => t.Holding)
            .ThenInclude(h => h.Currency)
            .Include(t => t.Holding)
            .ThenInclude(h => h.CryptoCurrency)
            .Include(t => t.FeeHolding)
            .ThenInclude(h => h.Currency)
            .Include(t => t.FeeHolding)
            .ThenInclude(h => h.CryptoCurrency)
            .OrderByDescending(t => t.DateTime)
            .ThenBy(t => t.Holding.Currency.Symbol)
            .ThenBy(t => t.Holding.CryptoCurrency.Symbol)
            .ThenBy(t => t.Direction)
            .AsSingleQuery();

        // Create a paged resultset
        var pageResult = await query.PaginateAsync(page, itemsPerPage ?? PagingConstants.DEFAULT_ITEMS_PER_PAGE, ct);
        var pageResultView = _mapper.Map<PagingViewModel<TransferListViewModel>>(pageResult);

        return Ok(pageResultView);
    }

    [HttpGet]
    [Route("{transferId}")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<ActionResult<TransferDetailViewModel>> GetTransfer(Guid transferId, CancellationToken ct)
    {
        var transfer = await _db.Transfers
            .AsNoTracking()
            .Where(t => t.Id == transferId)
            .Include(t => t.Holding)
            .ThenInclude(h => h.Currency)
            .Include(t => t.Holding)
            .ThenInclude(h => h.CryptoCurrency)
            .Include(t => t.FeeHolding)
            .ThenInclude(h => h.Currency)
            .Include(t => t.FeeHolding)
            .ThenInclude(h => h.CryptoCurrency)
            .Include(t => t.OppositeTransfer)
            .ThenInclude(t => t.Holding)
            .ThenInclude(h => h.Currency)
            .Include(t => t.OppositeTransfer)
            .ThenInclude(t => t.Holding)
            .ThenInclude(h => h.CryptoCurrency)
            .AsSplitQuery()
            .SingleOrDefaultAsync(ct);

        if (transfer == null)
            return NotFound();

        // TODO: Get referenced sources like wallets, bankaccounts and transactions on chain of bank

        var transferModel = _mapper.Map<TransferDetailViewModel>(transfer);

        // Calculate the exchange rate if it's a broker or transfer
        if (transfer.OppositeTransfer != null && exchangeTransferTypes.Contains(transfer.TransactionType))
        {
            transferModel.ExchangeRate = transfer.OppositeTransfer.TransferAmount / transfer.TransferAmount;
        }

        return Ok(transferModel);
    }

    /// <summary>
    /// Add a new Transfer.
    /// </summary>
    /// <param name="transfer"></param>
    /// <returns></returns>
    /// <exception cref="RestException"></exception>
    [HttpPost]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<ActionResult<TransferListViewModel>> PostTransfer([FromBody] TransferAddViewModel transfer)
    {
        if (!ModelState.IsValid)
            return BadRequest("Invalid data.");

        Holding holding = await _db.Holdings
            .Where(h => h.Id == transfer.FromHoldingId)
            .Include(h => h.Fund)
            .Include(h => h.Currency)
            .Include(h => h.CryptoCurrency)
            .AsSingleQuery()
            .FirstOrDefaultAsync();

        Holding feeHolding = transfer.FeeHoldingId.Equals(transfer.FromHoldingId)
            ? holding
            : await _db.Holdings
                .Where(h => h.Id == transfer.FeeHoldingId)
                .FirstOrDefaultAsync();

        if (holding == null)
        {
            _errorManager.AddValidationError(HttpStatusCode.NotFound, new ErrorInformationItem
            {
                Field = nameof(Transfer.Holding),
                Code = ErrorCodesStore.ReferencedRecordNotFound,
                Description = "Referenced holding not found."
            });
        }
        else if (holding?.PeriodClosedDateTime != null && !transfer.IsCorrection)
            _errorManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
            {
                Field = nameof(Holding.PeriodClosedDateTime),
                Code = ErrorCodesStore.BookingPeriodClosed,
                Description = "Can not add transfers to a closed booking period."
            });
        if (feeHolding == null)
        {
            _errorManager.AddValidationError(HttpStatusCode.NotFound, new ErrorInformationItem
            {
                Field = nameof(Transfer.Holding),
                Code = ErrorCodesStore.ReferencedRecordNotFound,
                Description = "Referenced fee holding not found."
            });
        }
        else if (transfer.TransferFee > 0 && feeHolding.PeriodClosedDateTime != null && !transfer.IsCorrection)
            _errorManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
            {
                Field = nameof(Holding.PeriodClosedDateTime),
                Code = ErrorCodesStore.BookingPeriodClosed,
                Description = "Can not subtract transfer fee on a closed booking period."
            });
        if (transfer.TransferFee < 0)
            _errorManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
            {
                Field = nameof(Transfer.TransferFee),
                Code = ErrorCodesStore.InvalidValue,
                Description = "Transfer fee can not be negative."
            });
        // Check bank transfer to be on a bank holding and a wallet transfer to be on a crypto holding
        if (transfer.TransactionType == TransactionType.Bank && string.IsNullOrEmpty(holding.CurrencyISOCode))
            _errorManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
            {
                Field = nameof(Transfer.TransactionType),
                Code = ErrorCodesStore.InvalidValue,
                Description = "Bank transfers can only be applied on bank holdings."
            });
        if (transfer.TransactionType == TransactionType.Wallet && holding.CryptoId == null)
            _errorManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
            {
                Field = nameof(Transfer.TransactionType),
                Code = ErrorCodesStore.InvalidValue,
                Description = "Wallet transfers can only be applied on crypto currency holdings."
            });

        _errorManager.ThrowOnErrors();

        using IDbContextTransaction transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            var newTransfer = _mapper.Map<Transfer>(transfer);

            // Fill the fields that are calculated
            newTransfer.Direction = directionInTransferTypes.Contains(newTransfer.TransactionType)
                ? TransferDirection.In
                : TransferDirection.Out;

            // Process the transfer
            if (newTransfer.TransactionType == TransactionType.Outflow ||
                newTransfer.TransactionType == TransactionType.Inflow)
            {
                // In- and out-flow is only calculated at the end of the period,
                // after the NAV value is calculated
                feeHolding.EndBalance -= transfer.TransferFee;
            }
            else if (newTransfer.Direction == TransferDirection.In)
            {
                holding.EndBalance += transfer.TransferAmount;
            }
            else
            {
                holding.EndBalance -= transfer.TransferAmount;
                feeHolding.EndBalance -= transfer.TransferFee;
            }

            // Now check the type of transfer and when needed create an opposite transfer.
            // Wallet, Broker, Transfers and Bank transfers must have an opposite transfer
            // and so a reference to the account/wallet.
            if (bidirectionalTransferTypes.Contains(newTransfer.TransactionType))
            {
                // Check for the ToAddress to be filled in
                if (string.IsNullOrEmpty(transfer.ToAddress))
                    _errorManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
                    {
                        Field = nameof(TransferAddViewModel.ToAddress),
                        Code = ErrorCodesStore.EmptyValue,
                        Description = "There should be a destination address for the transfer."
                    });

                Holding toHolding = exchangeTransferTypes.Contains(transfer.TransactionType)
                    ? await _db.Holdings
                        .Where(h => h.Id == transfer.ToHoldingId)
                        .Include(h => h.Fund)
                        .SingleOrDefaultAsync()
                    : holding;

                if (toHolding == null)
                {
                    _errorManager.AddValidationError(HttpStatusCode.NotFound, new ErrorInformationItem
                    {
                        Field = nameof(Transfer.Holding),
                        Code = ErrorCodesStore.ReferencedRecordNotFound,
                        Description = "References holding not found."
                    });
                }
                else
                {
                    if (toHolding.PeriodClosedDateTime != null)
                        _errorManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
                        {
                            Field = nameof(Holding.PeriodClosedDateTime),
                            Code = ErrorCodesStore.BookingPeriodClosed,
                            Description = "Can not add transfers to a closed booking period."
                        });
                    if (toHolding.FundId != holding.FundId)
                        _errorManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
                        {
                            Field = nameof(Holding.PeriodClosedDateTime),
                            Code = ErrorCodesStore.IllegalTransfer,
                            Description = "Can not move values between funds."
                        });
                }

                _errorManager.ThrowOnErrors();

                var oppositeTransfer = new Transfer()
                {
                    //OppositeTransferId = newTransfer.Id, // Do not fill yet, because of the circular dependency check!!
                    HoldingId = toHolding.Id,
                    BookingPeriod = newTransfer.BookingPeriod,
                    DateTime = newTransfer.DateTime,
                    TransactionType = newTransfer.TransactionType,
                    TransactionId = newTransfer.TransactionId,
                    Direction = TransferDirection.In,
                    TransactionSource = transfer.ToAddress,
                    TransferAmount = exchangeTransferTypes.Contains(transfer.TransactionType)
                        ? newTransfer.TransferAmount * transfer.ExchangeRate
                        : newTransfer.TransferAmount,
                    FeeHoldingId = newTransfer.FeeHoldingId,
                    TransferFee = 0, // Is taken from the original transfer
                    Reference = newTransfer.Reference
                };
                newTransfer.OppositeTransfer = oppositeTransfer;
                newTransfer.OppositeTransferId = oppositeTransfer.Id;

                // And calculate the new holding value
                toHolding.EndBalance += oppositeTransfer.TransferAmount;

                // To prevent circular dependency errors, follow the steps
                // below and savechanges on every step:
                // 1. First save the opposite transfer
                await _db.Transfers.AddAsync(oppositeTransfer);
                await _db.SaveChangesAsync();
                // 2. Then the new transfer, including the reference to opposite
                await _changeLogService.AddChangeLogAsync("Transfers", null, newTransfer);
                await _db.Transfers.AddAsync(newTransfer);
                await _db.SaveChangesAsync();
                // 3. And finally add the newTransfer id to the opposite transfer and save again
                oppositeTransfer.OppositeTransferId = newTransfer.Id;
                await _changeLogService.AddChangeLogAsync("Transfers", null, oppositeTransfer);
                await _db.SaveChangesAsync();

                // When the transfer is into or from an other fund (shares), also create
                // an inflow/outflow transaction in that fund.
                if (toHolding.SharesFundId != null)
                {
                    // From asset to shares - Inflow in referenced fund
                    var inflowHolding = await _fundService.GetOrCreateFundHolding(
                        (Guid)toHolding.SharesFundId,
                        holding.CryptoId,
                        holding.CurrencyISOCode,
                        holding.BookingPeriod);
                    var inflowTransfer = new Transfer()
                    {
                        OppositeTransferId = newTransfer.Id,
                        HoldingId = inflowHolding.Id,
                        BookingPeriod = inflowHolding.BookingPeriod,
                        DateTime = newTransfer.DateTime,
                        TransactionType = TransactionType.Inflow,
                        TransactionId = newTransfer.TransactionId,
                        Direction = TransferDirection.In,
                        TransactionSource = transfer.ToAddress,
                        TransferAmount = newTransfer.TransferAmount,
                        FeeHoldingId = inflowHolding.Id,
                        TransferFee = 0, // Is taken from the original transfer
                        Reference = newTransfer.Reference
                    };
                    await _changeLogService.AddChangeLogAsync("Transfers", null, inflowTransfer);
                    await _db.Transfers.AddAsync(inflowTransfer);
                    await _db.SaveChangesAsync();
                }
                if (holding.SharesFundId != null)
                {
                    // From shares to asset - Outflow from referenced fund
                    var outflowHolding = await _fundService.GetOrCreateFundHolding(
                        (Guid)holding.SharesFundId,
                        toHolding.CryptoId,
                        toHolding.CurrencyISOCode,
                        toHolding.BookingPeriod);
                    var outflowTransfer = new Transfer()
                    {
                        OppositeTransferId = newTransfer.Id,
                        HoldingId = outflowHolding.Id,
                        BookingPeriod = outflowHolding.BookingPeriod,
                        DateTime = newTransfer.DateTime,
                        TransactionType = TransactionType.Outflow,
                        TransactionId = newTransfer.TransactionId,
                        Direction = TransferDirection.Out,
                        TransactionSource = transfer.ToAddress,
                        Shares = (int)newTransfer.TransferAmount, // The amount is actually the number of shares
                        FeeHoldingId = outflowHolding.Id,
                        TransferFee = 0, // Is taken from the original transfer
                        Reference = newTransfer.Reference
                    };
                    await _changeLogService.AddChangeLogAsync("Transfers", null, outflowTransfer);
                    await _db.Transfers.AddAsync(outflowTransfer);
                    await _db.SaveChangesAsync();
                }
            }
            else
            {
                // Just save the single transfer
                await _changeLogService.AddChangeLogAsync("Transfers", null, newTransfer);
                await _db.Transfers.AddAsync(newTransfer);
                await _db.SaveChangesAsync();
            }

            _ = await _fundService.RecalcPercentages(holding.FundId, holding.BookingPeriod);

            transaction.Commit();

            return Ok(_mapper.Map<TransferListViewModel>(newTransfer));
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
    }

    [HttpPut]
    [Route("{transferId}")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<IActionResult> PutTransfer(Guid transferId, [FromBody] TransferEditViewModel transfer)
    {
        if (!ModelState.IsValid)
            return BadRequest("Invalid data.");

        var storedTransfer = await _db.Transfers
            .Where(t => t.Id == transferId)
            .FirstOrDefaultAsync();

        if (storedTransfer == null)
            return NotFound();

        if ((storedTransfer.TransactionType == TransactionType.Inflow || storedTransfer.TransactionType == TransactionType.Outflow) &&
            storedTransfer.OppositeTransferId != null)
        {
            // TODO: Send message to edit original transfer?? Only when full edit is fixed.
            //       Now we can edit the transfers seperately.
        }

        var storedHolding = await _db.Holdings
            .Where(h => h.Id == storedTransfer.HoldingId)
            .Include(h => h.Fund)
            .Include(h => h.Currency)
            .Include(h => h.CryptoCurrency)
            .AsSingleQuery()
            .SingleAsync();

        var storedFeeHolding = storedTransfer.FeeHoldingId.Equals(storedTransfer.HoldingId)
            ? storedHolding
            : await _db.Holdings
                .Where(h => h.Id == storedTransfer.FeeHoldingId)
                .SingleAsync();

        var newStoredFeeHolding = transfer.FeeHoldingId.Equals(storedFeeHolding.Id)
            ? storedFeeHolding
            : await _db.Holdings.FirstOrDefaultAsync(h => h.Id == transfer.FeeHoldingId);

        if (storedHolding.PeriodClosedDateTime != null)
            _errorManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
            {
                Field = nameof(Holding.PeriodClosedDateTime),
                Code = ErrorCodesStore.ReferencedRecordNotFound,
                Description = "Can not modify transfers on a closed booking period."
            });
        if (newStoredFeeHolding == null)
        {
            _errorManager.AddValidationError(HttpStatusCode.NotFound, new ErrorInformationItem
            {
                Field = nameof(Transfer.FeeHolding),
                Code = ErrorCodesStore.ReferencedRecordNotFound,
                Description = "Fee holding not found."
            });
        }
        else if (newStoredFeeHolding.PeriodClosedDateTime != null)
            _errorManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
            {
                Field = nameof(Holding.PeriodClosedDateTime),
                Code = ErrorCodesStore.ReferencedRecordNotFound,
                Description = "Can not modify transfer fee on a closed booking period."
            });
        if (transfer.TransferFee < 0)
            _errorManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
            {
                Field = nameof(Transfer.TransferFee),
                Code = ErrorCodesStore.InvalidValue,
                Description = "Transfer fee can not be negative."
            });

        _errorManager.ThrowOnErrors();

        using IDbContextTransaction transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            await _changeLogService.AddChangeLogAsync("Transfers", storedTransfer, transfer);

            // First revert the old EndBalance on the holding
            if (storedTransfer.TransactionType == TransactionType.Outflow ||
                storedTransfer.TransactionType == TransactionType.Inflow)
            {
                // In- and out-flow is only calculated at the end of the period,
                // after the NAV value is calculated
                storedFeeHolding.EndBalance -= storedTransfer.TransferFee;
            }
            else if (storedTransfer.Direction == TransferDirection.Out)
            {
                storedHolding.EndBalance += storedTransfer.TransferAmount;
                storedFeeHolding.EndBalance += storedTransfer.TransferFee;
            }
            else
            {
                storedHolding.EndBalance -= storedTransfer.TransferAmount;
            }

            // Prevent overriding the datetime when no value is entered
            if (transfer.DateTime == DateTime.MinValue)
            {
                transfer.DateTime = storedTransfer.DateTime;
            }

            _mapper.Map(transfer, storedTransfer);

            // Also get the optional opposite transfer and modify the values there
            if (bidirectionalTransferTypes.Contains(storedTransfer.TransactionType))
            {
                var oppositeTransfer = await _db.Transfers
                    .Where(t => t.Id == storedTransfer.OppositeTransferId)
                    .FirstOrDefaultAsync() ??
                    throw new RestException(HttpStatusCode.NotFound, new ErrorInformationItem
                    {
                        Field = nameof(Transfer.OppositeTransfer),
                        Code = ErrorCodesStore.ReferencedRecordNotFound,
                        Description = "Opposite transfer not found."
                    });
                var oppositeHolding = await _db.Holdings
                    .Where(h => h.Id == oppositeTransfer.HoldingId)
                    .FirstOrDefaultAsync();
                var oppositeFeeHolding = oppositeTransfer.FeeHoldingId.Equals(oppositeTransfer.HoldingId)
                    ? oppositeHolding
                    : await _db.Holdings
                        .Where(h => h.Id == oppositeTransfer.FeeHoldingId)
                        .FirstOrDefaultAsync();

                await _changeLogService.AddChangeLogAsync("Transfers", oppositeTransfer, transfer);

                // Now also revert the balance on the opposite holding
                if (oppositeTransfer.Direction == TransferDirection.Out)
                {
                    oppositeHolding.EndBalance += oppositeTransfer.TransferAmount;
                    oppositeFeeHolding.EndBalance += oppositeTransfer.TransferFee;
                }
                else
                {
                    oppositeHolding.EndBalance -= oppositeTransfer.TransferAmount;
                }

                storedTransfer.Direction = TransferDirection.Out;
                oppositeTransfer.Direction = TransferDirection.In;
                oppositeTransfer.DateTime = storedTransfer.DateTime;
                oppositeTransfer.TransactionSource = transfer.ToAddress;
                oppositeTransfer.TransactionId = storedTransfer.TransactionId;
                oppositeTransfer.TransferAmount = exchangeTransferTypes.Contains(storedTransfer.TransactionType)
                        ? storedTransfer.TransferAmount * transfer.ExchangeRate
                        : storedTransfer.TransferAmount;
                oppositeTransfer.TransferFee = 0;
                oppositeTransfer.Reference = storedTransfer.Reference;
                oppositeHolding.EndBalance += oppositeTransfer.TransferAmount;

                // TODO: And modify the in/outflow when there is a fund shares holding involved
                



            }

            // Process the transfer value
            if (storedTransfer.TransactionType == TransactionType.Outflow ||
                storedTransfer.TransactionType == TransactionType.Inflow)
            {
                // In- and out-flow is only calculated at the end of the period,
                // after the NAV value is calculated
                newStoredFeeHolding.EndBalance -= storedTransfer.TransferFee;
            }
            else if (storedTransfer.Direction == TransferDirection.In)
            {
                storedHolding.EndBalance += storedTransfer.TransferAmount;
            }
            else
            {
                storedHolding.EndBalance -= storedTransfer.TransferAmount;
                newStoredFeeHolding.EndBalance -= storedTransfer.TransferFee;
            }

            await _db.SaveChangesAsync();

            _ = await _fundService.RecalcPercentages(storedHolding.FundId, storedHolding.BookingPeriod);

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

    [HttpDelete]
    [Route("{transferId}")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<IActionResult> DeleteTransfer(Guid transferId)
    {
        var storedTransfer = await _db.Transfers
            .Where(c => c.Id == transferId)
            .FirstOrDefaultAsync();

        if (storedTransfer == null)
            return NotFound();

        if (storedTransfer.OppositeTransferId != null &&
            (storedTransfer.TransactionType == TransactionType.Inflow ||
             storedTransfer.TransactionType == TransactionType.Outflow)) 
        {
            // This is an infow or outflow from an other fund. Find the original transfer.
            return await DeleteTransfer((Guid)storedTransfer.OppositeTransferId);
        }

        var storedHolding = await _db.Holdings
            .Where(h => h.Id == storedTransfer.HoldingId)
            .Include(h => h.Fund)
            .Include(h => h.Currency)
            .Include(h => h.CryptoCurrency)
            .AsSingleQuery()
            .SingleAsync();

        var storedFeeHolding = storedTransfer.FeeHoldingId.Equals(storedTransfer.HoldingId)
            ? storedHolding
            : await _db.Holdings
                .Where(h => h.Id == storedTransfer.FeeHoldingId)
                .SingleAsync();

        using IDbContextTransaction transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            await _changeLogService.AddChangeLogAsync("Transfers", storedTransfer, null);

            // Remove reference transfers that are referencing without back reference
            var InOutTransfers = await _db.Transfers
                .Where(t => 
                    (t.OppositeTransferId == storedTransfer.Id && t.Id != storedTransfer.OppositeTransferId) || 
                    t.OppositeTransferId == storedTransfer.OppositeTransferId)
                .ToArrayAsync();
            _db.Transfers.RemoveRange(InOutTransfers);

            _db.Transfers.Remove(storedTransfer);

            // Revert the EndBalance on the holding
            if (storedTransfer.TransactionType == TransactionType.Outflow ||
                storedTransfer.TransactionType == TransactionType.Inflow)
            {
                storedFeeHolding.EndBalance += storedTransfer.TransferFee;
            }
            else if (storedTransfer.Direction == TransferDirection.Out)
            {
                storedHolding.EndBalance += storedTransfer.TransferAmount;
                storedFeeHolding.EndBalance += storedTransfer.TransferFee;
            }
            else
            {
                storedHolding.EndBalance -= storedTransfer.TransferAmount;
            }

            // TODO: Revert the shares from the in and outflow transfers?? Only when in closed bookingperiod. Also for update. Or is precessed on recalc bookingperiod
            //if (storedTransfer.TransactionType == TransactionType.Outflow ||
            //    storedTransfer.TransactionType == TransactionType.Inflow)
            //{
            //    if (storedTransfer.TransactionType == TransactionType.Outflow)
            //    {
            //        // Revert previous and decrease the shares on the fund.
            //        storedHolding.Fund.TotalShares += storedTransfer.Shares;
            //    }
            //    if (storedTransfer.TransactionType == TransactionType.Inflow)
            //    {
            //        // Revert privious value and increase the shares on the fund.
            //        storedHolding.Fund.TotalShares -= storedTransfer.Shares;
            //    }
            //}

            if (bidirectionalTransferTypes.Contains(storedTransfer.TransactionType))
            {
                var oppositeTransfer = await _db.Transfers
                    .Where(t => t.Id == storedTransfer.OppositeTransferId)
                    .FirstOrDefaultAsync() ??
                    throw new RestException(HttpStatusCode.NotFound, new ErrorInformationItem
                    {
                        Field = nameof(Transfer.OppositeTransfer),
                        Code = ErrorCodesStore.ReferencedRecordNotFound,
                        Description = "Opposite transfer not found."
                    });
                var oppositeHolding = await _db.Holdings
                    .Where(h => h.Id == oppositeTransfer.HoldingId)
                    .SingleAsync();

                var oppositeFeeHolding = oppositeTransfer.FeeHoldingId.Equals(oppositeTransfer.HoldingId)
                    ? oppositeHolding
                    : await _db.Holdings
                        .Where(h => h.Id == oppositeTransfer.FeeHoldingId)
                        .SingleAsync();

                await _changeLogService.AddChangeLogAsync("Transfers", oppositeTransfer, null);

                // Now also revert the balance on the opposite holding
                if (oppositeTransfer.Direction == TransferDirection.Out)
                {
                    oppositeHolding.EndBalance += oppositeTransfer.TransferAmount;
                    oppositeFeeHolding.EndBalance += oppositeTransfer.TransferFee;
                }
                else
                {
                    oppositeHolding.EndBalance -= oppositeTransfer.TransferAmount;
                }

                await _changeLogService.AddChangeLogAsync("Transfers", oppositeTransfer, null);
                _db.Transfers.Remove(oppositeTransfer);
            }

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
}
