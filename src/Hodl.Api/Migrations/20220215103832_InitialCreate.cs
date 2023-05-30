using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Hodl.Api.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Banks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    BankCode = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    Country = table.Column<string>(type: "text", nullable: false),
                    Url = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Banks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BlockchainNetworks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RPCUrl = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ChainID = table.Column<long>(type: "bigint", nullable: false),
                    ExplorerUrl = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlockchainNetworks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Group = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CryptoCurrencies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Decimals = table.Column<byte>(type: "smallint", nullable: false),
                    Symbol = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    Name = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    IsFiat = table.Column<bool>(type: "boolean", nullable: false),
                    IsLocked = table.Column<bool>(type: "boolean", nullable: false),
                    Icon = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CryptoCurrencies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Currencies",
                columns: table => new
                {
                    ISOCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Decimals = table.Column<byte>(type: "smallint", nullable: false),
                    Symbol = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    Name = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Location = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Default = table.Column<bool>(type: "boolean", nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Currencies", x => x.ISOCode);
                });

            migrationBuilder.CreateTable(
                name: "Exchanges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExchangeName = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Url = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Icon = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exchanges", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CryptoCategories",
                columns: table => new
                {
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    CryptoId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CryptoCategories", x => new { x.CategoryId, x.CryptoId });
                    table.ForeignKey(
                        name: "FK_CryptoCategories_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CryptoCategories_CryptoCurrencies_CryptoId",
                        column: x => x.CryptoId,
                        principalTable: "CryptoCurrencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Listings",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CryptoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Source = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    TimeStamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CmcRank = table.Column<int>(type: "integer", nullable: false),
                    NumMarketPairs = table.Column<int>(type: "integer", nullable: false),
                    CirculatingSupply = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalSupply = table.Column<decimal>(type: "numeric", nullable: false),
                    MaxSupply = table.Column<decimal>(type: "numeric", nullable: false),
                    USDPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    BTCPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    Volume_24h = table.Column<decimal>(type: "numeric", nullable: false),
                    VolumeChange_24h = table.Column<decimal>(type: "numeric", nullable: false),
                    PercentChange_1h = table.Column<decimal>(type: "numeric", nullable: false),
                    PercentChange_24h = table.Column<decimal>(type: "numeric", nullable: false),
                    PercentChange_7d = table.Column<decimal>(type: "numeric", nullable: false),
                    MarketCap = table.Column<decimal>(type: "numeric", nullable: false),
                    MarketCapDominance = table.Column<decimal>(type: "numeric", nullable: false),
                    FDMarketCap = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Listings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Listings_CryptoCurrencies_CryptoId",
                        column: x => x.CryptoId,
                        principalTable: "CryptoCurrencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Pairs",
                columns: table => new
                {
                    PairString = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    FromCryptoId = table.Column<Guid>(type: "uuid", nullable: false),
                    ToCryptoId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pairs", x => x.PairString);
                    table.ForeignKey(
                        name: "FK_Pairs_CryptoCurrencies_FromCryptoId",
                        column: x => x.FromCryptoId,
                        principalTable: "CryptoCurrencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Pairs_CryptoCurrencies_ToCryptoId",
                        column: x => x.ToCryptoId,
                        principalTable: "CryptoCurrencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CurrencyRates",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CyrrencyISOCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    USDPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    TimeStamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Source = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurrencyRates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CurrencyRates_Currencies_CyrrencyISOCode",
                        column: x => x.CyrrencyISOCode,
                        principalTable: "Currencies",
                        principalColumn: "ISOCode",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Funds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FundName = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    NormalizedFundName = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    DateStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MaxVolume = table.Column<int>(type: "integer", nullable: false),
                    LayerStrategy = table.Column<string>(type: "text", nullable: false),
                    ReportingCurrencyCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    PrimaryCryptoCurrencyId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfitPercentage = table.Column<int>(type: "integer", nullable: false),
                    TotalValue = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalShares = table.Column<int>(type: "integer", nullable: false),
                    ShareValueHWM = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Funds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Funds_CryptoCurrencies_PrimaryCryptoCurrencyId",
                        column: x => x.PrimaryCryptoCurrencyId,
                        principalTable: "CryptoCurrencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Funds_Currencies_ReportingCurrencyCode",
                        column: x => x.ReportingCurrencyCode,
                        principalTable: "Currencies",
                        principalColumn: "ISOCode",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExchangeAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExchangeId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountKey = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Name = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ParentAccountId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExchangeAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExchangeAccounts_ExchangeAccounts_ParentAccountId",
                        column: x => x.ParentAccountId,
                        principalTable: "ExchangeAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExchangeAccounts_Exchanges_ExchangeId",
                        column: x => x.ExchangeId,
                        principalTable: "Exchanges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BankAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BankId = table.Column<Guid>(type: "uuid", nullable: false),
                    FundId = table.Column<Guid>(type: "uuid", nullable: false),
                    HolderName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IBAN = table.Column<string>(type: "text", nullable: false),
                    BIC = table.Column<string>(type: "text", nullable: false),
                    CurrencyCode = table.Column<string>(type: "character varying(3)", nullable: false),
                    Balance = table.Column<decimal>(type: "numeric", nullable: false),
                    TimeStamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BankAccounts_Banks_BankId",
                        column: x => x.BankId,
                        principalTable: "Banks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BankAccounts_Currencies_CurrencyCode",
                        column: x => x.CurrencyCode,
                        principalTable: "Currencies",
                        principalColumn: "ISOCode",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BankAccounts_Funds_FundId",
                        column: x => x.FundId,
                        principalTable: "Funds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DailyNavs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FundId = table.Column<Guid>(type: "uuid", nullable: false),
                    DateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TotalValue = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalShares = table.Column<int>(type: "integer", nullable: false),
                    ShareHWM = table.Column<decimal>(type: "numeric", nullable: false),
                    ShareGross = table.Column<decimal>(type: "numeric", nullable: false),
                    ShareNAV = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyNavs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyNavs_Funds_FundId",
                        column: x => x.FundId,
                        principalTable: "Funds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FundCategory",
                columns: table => new
                {
                    FundId = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    MinPercentage = table.Column<byte>(type: "smallint", nullable: false),
                    MaxPercentage = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FundCategory", x => new { x.FundId, x.CategoryId });
                    table.ForeignKey(
                        name: "FK_FundCategory_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FundCategory_Funds_FundId",
                        column: x => x.FundId,
                        principalTable: "Funds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FundLayers",
                columns: table => new
                {
                    FundId = table.Column<Guid>(type: "uuid", nullable: false),
                    LayerIndex = table.Column<byte>(type: "smallint", nullable: false),
                    Name = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    AimPercentage = table.Column<byte>(type: "smallint", nullable: false),
                    AlertRangeLow = table.Column<byte>(type: "smallint", nullable: false),
                    AlertRangeHigh = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FundLayers", x => new { x.FundId, x.LayerIndex });
                    table.ForeignKey(
                        name: "FK_FundLayers_Funds_FundId",
                        column: x => x.FundId,
                        principalTable: "Funds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Holdings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PreviousHoldingId = table.Column<Guid>(type: "uuid", nullable: true),
                    FundId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrencyISOCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    CryptoId = table.Column<Guid>(type: "uuid", nullable: true),
                    BookingPeriod = table.Column<string>(type: "nchar(6)", maxLength: 6, nullable: false),
                    PeriodClosedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StartDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StartBalance = table.Column<decimal>(type: "numeric", nullable: false),
                    StartUSDPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    StartBTCPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    EndDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndBalance = table.Column<decimal>(type: "numeric", nullable: false),
                    EndUSDPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    EndBTCPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    LayerIndex = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Holdings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Holdings_CryptoCurrencies_CryptoId",
                        column: x => x.CryptoId,
                        principalTable: "CryptoCurrencies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Holdings_Currencies_CurrencyISOCode",
                        column: x => x.CurrencyISOCode,
                        principalTable: "Currencies",
                        principalColumn: "ISOCode");
                    table.ForeignKey(
                        name: "FK_Holdings_Funds_FundId",
                        column: x => x.FundId,
                        principalTable: "Funds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Holdings_Holdings_PreviousHoldingId",
                        column: x => x.PreviousHoldingId,
                        principalTable: "Holdings",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Wallets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CryptoId = table.Column<Guid>(type: "uuid", nullable: false),
                    FundId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExchangeAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    BlockchainNetworkId = table.Column<Guid>(type: "uuid", nullable: false),
                    Address = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wallets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Wallets_BlockchainNetworks_BlockchainNetworkId",
                        column: x => x.BlockchainNetworkId,
                        principalTable: "BlockchainNetworks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Wallets_CryptoCurrencies_CryptoId",
                        column: x => x.CryptoId,
                        principalTable: "CryptoCurrencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Wallets_ExchangeAccounts_ExchangeAccountId",
                        column: x => x.ExchangeAccountId,
                        principalTable: "ExchangeAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Wallets_Funds_FundId",
                        column: x => x.FundId,
                        principalTable: "Funds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Transfers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    HoldingId = table.Column<Guid>(type: "uuid", nullable: false),
                    OppositeTransferId = table.Column<long>(type: "bigint", nullable: true),
                    BookingPeriod = table.Column<string>(type: "nchar(6)", maxLength: 6, nullable: false),
                    DateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TransactionType = table.Column<string>(type: "text", nullable: false),
                    TransactionSource = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TransactionId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Direction = table.Column<string>(type: "text", nullable: false),
                    TransferAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    TransferFee = table.Column<decimal>(type: "numeric", nullable: false),
                    Reference = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transfers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transfers_Holdings_HoldingId",
                        column: x => x.HoldingId,
                        principalTable: "Holdings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Transfers_Transfers_OppositeTransferId",
                        column: x => x.OppositeTransferId,
                        principalTable: "Transfers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WalletId = table.Column<Guid>(type: "uuid", nullable: false),
                    DateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PairString = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Side = table.Column<string>(type: "text", nullable: false),
                    State = table.Column<string>(type: "text", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    Executed = table.Column<decimal>(type: "numeric", nullable: false),
                    AveragePrice = table.Column<decimal>(type: "numeric", nullable: false),
                    Total = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalFee = table.Column<decimal>(type: "numeric", nullable: false),
                    FeeCurrencyId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_CryptoCurrencies_FeeCurrencyId",
                        column: x => x.FeeCurrencyId,
                        principalTable: "CryptoCurrencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Orders_Pairs_PairString",
                        column: x => x.PairString,
                        principalTable: "Pairs",
                        principalColumn: "PairString",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Orders_Wallets_WalletId",
                        column: x => x.WalletId,
                        principalTable: "Wallets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderFundings",
                columns: table => new
                {
                    OrderId = table.Column<long>(type: "bigint", nullable: false),
                    FundId = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingPeriod = table.Column<string>(type: "nchar(6)", maxLength: 6, nullable: false),
                    OrderPercentage = table.Column<decimal>(type: "numeric", nullable: false),
                    OrderAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    OrderTotal = table.Column<decimal>(type: "numeric", nullable: false),
                    OrderFee = table.Column<decimal>(type: "numeric", nullable: false),
                    FeeCurrencyId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderFundings", x => new { x.OrderId, x.FundId });
                    table.ForeignKey(
                        name: "FK_OrderFundings_CryptoCurrencies_FeeCurrencyId",
                        column: x => x.FeeCurrencyId,
                        principalTable: "CryptoCurrencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderFundings_Funds_FundId",
                        column: x => x.FundId,
                        principalTable: "Funds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderFundings_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Trades",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderId = table.Column<long>(type: "bigint", nullable: false),
                    ExchangeAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransactionId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    DateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    Executed = table.Column<decimal>(type: "numeric", nullable: false),
                    Total = table.Column<decimal>(type: "numeric", nullable: false),
                    Fee = table.Column<decimal>(type: "numeric", nullable: false),
                    FeeCurrency = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Trades_ExchangeAccounts_ExchangeAccountId",
                        column: x => x.ExchangeAccountId,
                        principalTable: "ExchangeAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Trades_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BankAccounts_BankId",
                table: "BankAccounts",
                column: "BankId");

            migrationBuilder.CreateIndex(
                name: "IX_BankAccounts_CurrencyCode",
                table: "BankAccounts",
                column: "CurrencyCode");

            migrationBuilder.CreateIndex(
                name: "IX_BankAccounts_FundId",
                table: "BankAccounts",
                column: "FundId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Group_Name",
                table: "Categories",
                columns: new[] { "Group", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_CryptoCategories_CryptoId",
                table: "CryptoCategories",
                column: "CryptoId");

            migrationBuilder.CreateIndex(
                name: "IX_CryptoCurrencies_Symbol_Name",
                table: "CryptoCurrencies",
                columns: new[] { "Symbol", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyRates_CyrrencyISOCode_TimeStamp",
                table: "CurrencyRates",
                columns: new[] { "CyrrencyISOCode", "TimeStamp" });

            migrationBuilder.CreateIndex(
                name: "IX_DailyNavs_FundId_DateTime",
                table: "DailyNavs",
                columns: new[] { "FundId", "DateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeAccounts_ExchangeId",
                table: "ExchangeAccounts",
                column: "ExchangeId");

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeAccounts_ParentAccountId",
                table: "ExchangeAccounts",
                column: "ParentAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_FundCategory_CategoryId",
                table: "FundCategory",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_FundCategory_FundId_CategoryId",
                table: "FundCategory",
                columns: new[] { "FundId", "CategoryId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FundLayers_FundId_LayerIndex",
                table: "FundLayers",
                columns: new[] { "FundId", "LayerIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_Funds_NormalizedFundName",
                table: "Funds",
                column: "NormalizedFundName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Funds_PrimaryCryptoCurrencyId",
                table: "Funds",
                column: "PrimaryCryptoCurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_Funds_ReportingCurrencyCode",
                table: "Funds",
                column: "ReportingCurrencyCode");

            migrationBuilder.CreateIndex(
                name: "IX_Holdings_CryptoId",
                table: "Holdings",
                column: "CryptoId");

            migrationBuilder.CreateIndex(
                name: "IX_Holdings_CurrencyISOCode",
                table: "Holdings",
                column: "CurrencyISOCode");

            migrationBuilder.CreateIndex(
                name: "IX_Holdings_FundId_BookingPeriod_CurrencyISOCode",
                table: "Holdings",
                columns: new[] { "FundId", "BookingPeriod", "CurrencyISOCode" });

            migrationBuilder.CreateIndex(
                name: "IX_Holdings_PreviousHoldingId",
                table: "Holdings",
                column: "PreviousHoldingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Listings_CryptoId_TimeStamp",
                table: "Listings",
                columns: new[] { "CryptoId", "TimeStamp" });

            migrationBuilder.CreateIndex(
                name: "IX_OrderFundings_FeeCurrencyId",
                table: "OrderFundings",
                column: "FeeCurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderFundings_FundId",
                table: "OrderFundings",
                column: "FundId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_FeeCurrencyId",
                table: "Orders",
                column: "FeeCurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_PairString_State",
                table: "Orders",
                columns: new[] { "PairString", "State" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_PairString_Type_DateTime",
                table: "Orders",
                columns: new[] { "PairString", "Type", "DateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_WalletId",
                table: "Orders",
                column: "WalletId");

            migrationBuilder.CreateIndex(
                name: "IX_Pairs_FromCryptoId",
                table: "Pairs",
                column: "FromCryptoId");

            migrationBuilder.CreateIndex(
                name: "IX_Pairs_ToCryptoId",
                table: "Pairs",
                column: "ToCryptoId");

            migrationBuilder.CreateIndex(
                name: "IX_Trades_ExchangeAccountId",
                table: "Trades",
                column: "ExchangeAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Trades_OrderId_ExchangeAccountId_DateTime",
                table: "Trades",
                columns: new[] { "OrderId", "ExchangeAccountId", "DateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_HoldingId",
                table: "Transfers",
                column: "HoldingId");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_HoldingId_BookingPeriod_DateTime",
                table: "Transfers",
                columns: new[] { "HoldingId", "BookingPeriod", "DateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_OppositeTransferId",
                table: "Transfers",
                column: "OppositeTransferId");

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_BlockchainNetworkId",
                table: "Wallets",
                column: "BlockchainNetworkId");

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_CryptoId",
                table: "Wallets",
                column: "CryptoId");

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_ExchangeAccountId",
                table: "Wallets",
                column: "ExchangeAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_FundId",
                table: "Wallets",
                column: "FundId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BankAccounts");

            migrationBuilder.DropTable(
                name: "CryptoCategories");

            migrationBuilder.DropTable(
                name: "CurrencyRates");

            migrationBuilder.DropTable(
                name: "DailyNavs");

            migrationBuilder.DropTable(
                name: "FundCategory");

            migrationBuilder.DropTable(
                name: "FundLayers");

            migrationBuilder.DropTable(
                name: "Listings");

            migrationBuilder.DropTable(
                name: "OrderFundings");

            migrationBuilder.DropTable(
                name: "Trades");

            migrationBuilder.DropTable(
                name: "Transfers");

            migrationBuilder.DropTable(
                name: "Banks");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Holdings");

            migrationBuilder.DropTable(
                name: "Pairs");

            migrationBuilder.DropTable(
                name: "Wallets");

            migrationBuilder.DropTable(
                name: "BlockchainNetworks");

            migrationBuilder.DropTable(
                name: "ExchangeAccounts");

            migrationBuilder.DropTable(
                name: "Funds");

            migrationBuilder.DropTable(
                name: "Exchanges");

            migrationBuilder.DropTable(
                name: "CryptoCurrencies");

            migrationBuilder.DropTable(
                name: "Currencies");
        }
    }
}
