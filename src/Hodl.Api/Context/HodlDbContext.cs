using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Reflection;

namespace Hodl.Api.Contexts;

public class HodlDbContext : IdentityDbContext<AppUser, AppRole, Guid>, IDbContextInitialDataProvider
{
    public HodlDbContext() : base()
    {
    }

    public HodlDbContext(DbContextOptions<HodlDbContext> options) : base(options)
    {
    }

    #region Users and log
    public virtual DbSet<AppUser> AppUsers { get; set; }

    public virtual DbSet<AppChangeLog> AppChanges { get; set; }

    public virtual DbSet<AppConfig> AppConfigs { get; set; }

    #endregion

    #region Currencies
    public virtual DbSet<Currency> Currencies { get; set; }
    public virtual DbSet<CurrencyRate> CurrencyRates { get; set; }
    public virtual DbSet<CryptoCurrency> CryptoCurrencies { get; set; }
    public virtual DbSet<Listing> Listings { get; set; }
    #endregion

    #region Funds and holdings
    public virtual DbSet<Fund> Funds { get; set; }
    public virtual DbSet<FundOwner> FundOwners { get; set; }
    public virtual DbSet<FundLayer> FundLayers { get; set; }
    public virtual DbSet<FundCategory> FundCategories { get; set; }
    public virtual DbSet<Category> Categories { get; set; }
    public virtual DbSet<CryptoCategory> CryptoCategories { get; set; }
    public virtual DbSet<Holding> Holdings { get; set; }
    public virtual DbSet<Nav> Navs { get; set; }
    #endregion

    #region ExternalAccounts
    public virtual DbSet<Exchange> Exchanges { get; set; }
    public virtual DbSet<ExchangeAccount> ExchangeAccounts { get; set; }
    public virtual DbSet<BlockchainNetwork> BlockchainNetworks { get; set; }
    public virtual DbSet<TokenContract> TokenContracts { get; set; }
    public virtual DbSet<WalletBalance> WalletBalances { get; set; }
    public virtual DbSet<Wallet> Wallets { get; set; }
    public virtual DbSet<Bank> Banks { get; set; }
    public virtual DbSet<BankAccount> BankAccounts { get; set; }
    public virtual DbSet<BankBalance> BankBalances { get; set; }

    #endregion

    #region Trading
    public virtual DbSet<Transfer> Transfers { get; set; }
    public virtual DbSet<Pair> Pairs { get; set; }
    public virtual DbSet<Order> Orders { get; set; }
    public virtual DbSet<Trade> Trades { get; set; }
    public virtual DbSet<OrderFunding> OrderFundings { get; set; }
    #endregion

    #region Notification system
    public virtual DbSet<UserNotification> UserNotifications { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AppUser>()
            .Ignore(c => c.PhoneNumber)
            .Ignore(c => c.PhoneNumberConfirmed);

        // First the partial keys
        modelBuilder.Entity<FundLayer>()
            .HasKey(fl => new { fl.FundId, fl.LayerIndex });

        modelBuilder.Entity<FundCategory>()
            .HasKey(fc => new { fc.FundId, fc.CategoryId });

        modelBuilder.Entity<OrderFunding>()
            .HasKey(of => new { of.OrderId, of.FundId });

        // Field conversions
        modelBuilder.Entity<Nav>()
            .Property(e => e.Type)
            .HasConversion(
                v => v.ToString(),
                v => v.ParseEnum<NavType>());

        modelBuilder.Entity<Transfer>()
            .Property(e => e.TransactionType)
            .HasConversion(
                v => v.ToString(),
                v => v.ParseEnum<TransactionType>());

        modelBuilder.Entity<Transfer>()
            .Property(e => e.Direction)
            .HasConversion(
                v => v.ToString(),
                v => v.ParseEnum<TransferDirection>());

        modelBuilder.Entity<Order>()
            .Property(e => e.Direction)
            .HasConversion(
                v => v.ToString(),
                v => v.ParseEnum<OrderDirection>());

        modelBuilder.Entity<Order>()
            .Property(e => e.State)
            .HasConversion(
                v => v.ToString(),
                v => v.ParseEnum<OrderState>());

        // Relations
        modelBuilder.Entity<Currency>()
            .HasMany(c => c.CurrencyRates)
            .WithOne(cr => cr.Currency)
            //.HasForeignKey(cr => cr.CurrencyISOCode) <- Foreign keys defined in the classes
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CryptoCurrency>()
            .HasMany(cc => cc.Listings)
            .WithOne(l => l.CryptoCurrency)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Fund>()
            .HasOne(f => f.FundOwner)
            .WithMany(fo => fo.Funds)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Fund>()
            .HasMany(f => f.Layers)
            .WithOne(l => l.Fund)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Fund>()
            .HasMany(f => f.Holdings)
            .WithOne(h => h.Fund)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Fund>()
            .HasMany(f => f.DailyNavs)
            .WithOne(h => h.Fund)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Fund>()
            .HasMany(f => f.Orders)
            .WithMany(o => o.Funds)
            .UsingEntity<OrderFunding>(
                j => j
                    .HasOne(of => of.Order)
                    .WithMany(o => o.OrderFundings)
                    .OnDelete(DeleteBehavior.Cascade),
                j => j
                    .HasOne(of => of.Fund)
                    .WithMany(f => f.OrderFundings),
                j => j
                    .HasKey(of => new { of.OrderId, of.FundId })
                );

        modelBuilder.Entity<Category>()
            .HasMany(c => c.Funds)
            .WithMany(f => f.Categories)
            .UsingEntity<FundCategory>(
                j => j
                    .HasOne(fc => fc.Fund)
                    .WithMany(f => f.FundCategories)
                    .OnDelete(DeleteBehavior.Cascade),
                j => j
                    .HasOne(fc => fc.Category)
                    .WithMany(c => c.FundCategories)
                    .OnDelete(DeleteBehavior.Cascade),
                j => j
                    .HasKey(fc => new { fc.CategoryId, fc.FundId })
                );

        modelBuilder.Entity<Category>()
            .HasMany(c => c.CryptoCurrencies)
            .WithMany(f => f.Categories)
            .UsingEntity<CryptoCategory>(
                j => j
                    .HasOne(cc => cc.CryptoCurrency)
                    .WithMany()
                    .OnDelete(DeleteBehavior.Cascade),
                j => j
                    .HasOne(cc => cc.Category)
                    .WithMany(c => c.CryptoCategories)
                    .OnDelete(DeleteBehavior.Cascade),
                j => j
                    .HasKey(cc => new { cc.CategoryId, cc.CryptoId })
                );

        modelBuilder.Entity<Holding>()
            .HasOne(h => h.PreviousHolding)
            .WithOne(h => h.NextHolding);

        modelBuilder.Entity<Holding>()
            .HasOne(h => h.Currency)
            .WithMany(c => c.Holdings);

        modelBuilder.Entity<Holding>()
            .HasOne(h => h.CurrencyRate)
            .WithMany();

        modelBuilder.Entity<Holding>()
            .HasOne(h => h.CryptoCurrency)
            .WithMany(c => c.Holdings);

        modelBuilder.Entity<Holding>()
            .HasOne(h => h.Listing)
            .WithMany();

        modelBuilder.Entity<Nav>()
            .HasOne(n => n.CurrencyRate)
            .WithMany()
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Transfer>()
            .HasOne(t => t.Holding)
            .WithMany(h => h.Transfers)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Transfer>()
            .HasOne(t => t.FeeHolding)
            .WithMany()
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ExchangeAccount>()
            .HasOne(ea => ea.Exchange)
            .WithMany(e => e.ExchangeAccounts);

        modelBuilder.Entity<ExchangeAccount>()
            .HasOne(ea => ea.ParentAccount)
            .WithMany(ea => ea.ChildAccounts);

        modelBuilder.Entity<Pair>()
            .HasOne(p => p.BaseAsset)
            .WithMany()
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Pair>()
            .HasOne(p => p.QuoteAsset)
            .WithMany()
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Order>()
            .HasOne(o => o.ExchangeAccount)
            .WithMany(ea => ea.Orders);
        modelBuilder.Entity<Order>()
            .HasOne(o => o.Wallet)
            .WithMany(w => w.Orders);

        modelBuilder.Entity<Order>()
            .HasOne(o => o.BaseAsset)
            .WithMany();
        modelBuilder.Entity<Order>()
            .HasOne(o => o.QuoteAsset)
            .WithMany();

        modelBuilder.Entity<Trade>()
            .HasOne(t => t.Order)
            .WithMany(o => o.Trades);

        modelBuilder.Entity<Trade>()
            .HasOne(o => o.FeeCurrency)
            .WithMany();

        modelBuilder.Entity<WalletBalance>()
            .HasOne(wb => wb.BlockchainNetwork)
            .WithMany(bn => bn.WalletBalances);
        modelBuilder.Entity<WalletBalance>()
            .HasOne(wb => wb.CryptoCurrency)
            .WithMany();
        modelBuilder.Entity<WalletBalance>()
            .HasOne(wb => wb.Wallet)
            .WithMany(w => w.WalletBalances);

        modelBuilder.Entity<Wallet>()
            .HasOne(t => t.ExchangeAccount)
            .WithMany(ea => ea.Wallets);

        modelBuilder.Entity<BankAccount>()
            .HasOne(ba => ba.Bank)
            .WithMany(b => b.BankAccounts);
        modelBuilder.Entity<BankAccount>()
            .HasOne(ba => ba.Fund)
            .WithMany(f => f.BankAccounts);

        modelBuilder.Entity<BankBalance>()
            .HasOne(bb => bb.BankAccount)
            .WithMany(ba => ba.Balances);

        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Insert only static data
        modelBuilder.Entity<AppRole>().HasData(
            new AppRole { Id = new Guid("c1a6e520-01f1-4fed-85e1-a2a358d7fb02"), Name = "Admin", NormalizedName = "ADMIN" },
            new AppRole { Id = new Guid("526198db-4e9f-4200-bbf9-82ecfe39330c"), Name = "Trader", NormalizedName = "TRADER" },
            new AppRole { Id = new Guid("d12fa1e4-409f-42f6-900c-dbd8295ca1cf"), Name = "LeadTrader", NormalizedName = "LEADTRADER" },
            new AppRole { Id = new Guid("bc1bd304-deee-4ecb-b00e-420484984c83"), Name = "Sales", NormalizedName = "SALES" },
            new AppRole { Id = new Guid("0aff7b76-053d-43d9-aa1b-308df84c32e1"), Name = "HeadSales", NormalizedName = "HEADSALES" },
            new AppRole { Id = new Guid("420d65a9-f385-4f19-9c25-b48eaa4ee2dd"), Name = "Client", NormalizedName = "CLIENT" }
            );
    }

    public void AddInitialData()
    {
        if (!Currencies.Any())
        {
            Currencies.AddRange(new[] {
                new Currency { ISOCode = "EUR", Name = "Euro", Decimals = 2, Symbol = "€", Location = "Europe", Default = true, Active = true },
                new Currency { ISOCode = "USD", Name = "US dollar", Decimals = 2, Symbol = "$", Location = "United States", Default = false, Active = true },
                new Currency { ISOCode = "GBP", Name = "British pound", Decimals = 2, Symbol = "£", Location = "United Kingdom", Default = true, Active = true },
                new Currency { ISOCode = "CHF", Name = "Swiss franc", Decimals = 2, Symbol = "Fr.", Location = "Switzerland, Liechtenstein", Default = true, Active = true }
            });
            SaveChanges();
            CurrencyRates.AddRange(new[] {
                new CurrencyRate { CurrencyISOCode = "EUR", TimeStamp = new DateTime(2022, 5, 2, 12, 0, 0, DateTimeKind.Utc), Source = "https://nl.iban.com/exchange-rates", USDRate = 1.0524M },
                new CurrencyRate { CurrencyISOCode = "USD", TimeStamp = new DateTime(2022, 5, 2, 12, 0, 0, DateTimeKind.Utc), Source = "https://nl.iban.com/exchange-rates", USDRate = 1.0000M },
                new CurrencyRate { CurrencyISOCode = "GBP", TimeStamp = new DateTime(2022, 5, 2, 12, 0, 0, DateTimeKind.Utc), Source = "https://nl.iban.com/exchange-rates", USDRate = 1.2557M },
                new CurrencyRate { CurrencyISOCode = "CHF", TimeStamp = new DateTime(2022, 5, 2, 12, 0, 0, DateTimeKind.Utc), Source = "https://nl.iban.com/exchange-rates", USDRate = 1.0253M }
            });
            SaveChanges();
        }

        if (!CryptoCurrencies.Any())
        {
            CryptoCurrencies.AddRange(new[] {
                new CryptoCurrency { Id = new Guid("fb3194c2-e732-4219-89b9-bc63eea2a861"), Decimals = 8, Symbol = "BTC", Name = "Bitcoin", Active = true, IsStableCoin = false, IsLocked = false, Icon = IconFromAssembly("Hodl.Api.Resources.Images.crypto-bitcoin.png") },
                new CryptoCurrency { Id = new Guid("796923d5-a790-40d1-9074-c8fe3c25d049"), Decimals = 18, Symbol = "ETH", Name = "Ethereum", Active = true, IsStableCoin = false, IsLocked = false, Icon = IconFromAssembly("Hodl.Api.Resources.Images.crypto-eth.png") },
                new CryptoCurrency { Id = new Guid("73f3f464-e442-4dcd-8b9c-7cf902931e89"), Decimals = 18, Symbol = "BNB", Name = "BNB", Active = true, IsStableCoin = false, IsLocked = false, Icon = IconFromAssembly("Hodl.Api.Resources.Images.crypto-bnb.png") },
                new CryptoCurrency { Id = new Guid("a5a7e6cb-ccdf-49ea-a4e8-6b6d6467004a"), Decimals = 6, Symbol = "USDT", Name = "Tether", Active = true, IsStableCoin = true, IsLocked = false, Icon = IconFromAssembly("Hodl.Api.Resources.Images.crypto-usdt.png") },
            });
            SaveChanges();
        }

        if (!AppConfigs.Any(c => c.Name == "Crypto.Listing.HistorySymbols"))
        {
            AppConfigs.Add(
                new AppConfig { Name = "Crypto.Listing.HistorySymbols", Value = "[\"BTC\",\"ETH\",\"BNB\",\"USDT\"]", DateTime = DateTime.UtcNow }
            );
            SaveChanges();
        }

        // Add default supported blockchains
        if (!BlockchainNetworks.Any())
        {
            BlockchainNetworks.AddRange(new[] {
                // Mainnets
                new BlockchainNetwork { Id = new Guid("3c86ffad-e283-42ad-ae10-cfd3db93a208"), Name = "Bitcoin mainnet", RPCUrl = "", ChainID = 0, ExplorerUrl = "https://btcscan.org/", CurrencySymbol = "BTC", IsTestNet = false },
                new BlockchainNetwork { Id = new Guid("56181158-7d68-42f8-babc-0ed8af6d7054"), Name = "Ethereum mainnet", RPCUrl = "https://mainnet.infura.io/v3/", ChainID = 1, ExplorerUrl = "https://etherscan.io", CurrencySymbol = "ETH", IsTestNet = false },
                new BlockchainNetwork { Id = new Guid("1c800a3c-769d-4d7c-93c7-047f5a660703"), Name = "Binance Smart Chain", RPCUrl = "https://bsc-dataseed.binance.org/", ChainID = 56, ExplorerUrl = "https://bscscan.com", CurrencySymbol = "BNB", IsTestNet = false },
                new BlockchainNetwork { Id = new Guid("a3001aab-344c-480e-a7f3-b83b0c36021d"), Name = "Polygon mainnet", RPCUrl = "https://rpc-mainnet.matic.network", ChainID = 137, ExplorerUrl = "https://polygonscan.com/", CurrencySymbol = "MATIC", IsTestNet = false },
                // Testnets
                new BlockchainNetwork { Id = new Guid("1755173f-112f-4c82-843f-8423cdeb5ab3"), Name = "Ethereum Sepolia testnet", RPCUrl = "https://rpc.sepolia.dev", ChainID = 11155111, ExplorerUrl = "https://sepolia.etherscan.io/", CurrencySymbol = "ETH", IsTestNet = true },
                new BlockchainNetwork { Id = new Guid("d7b4c753-0768-4c06-abc9-68f9703ef68f"), Name = "Ethereum Goerli testnet", RPCUrl = "https://goerli.infura.io/v3/", ChainID = 5, ExplorerUrl = "https://goerli.etherscan.io/", CurrencySymbol = "ETH", IsTestNet = true },
                new BlockchainNetwork { Id = new Guid("1c29639b-2859-4445-a208-948f5f330266"), Name = "Binance Smart Chain testnet", RPCUrl = "https://data-seed-prebsc-1-s1.binance.org:8545/", ChainID = 97, ExplorerUrl = "https://testnet.bscscan.com", CurrencySymbol = "BNB", IsTestNet = true },
                new BlockchainNetwork { Id = new Guid("90a7fbc6-aaf8-4d4d-96b8-c5347b4bc625"), Name = "Polygon Mumbai testnet", RPCUrl = "https://rpc-mumbai.maticvigil.com/", ChainID = 80001, ExplorerUrl = "https://mumbai.polygonscan.com/", CurrencySymbol = "MATIC", IsTestNet = true },
            });
            SaveChanges();
        }
        // Add additional blockchains
        if (!BlockchainNetworks.Any(n => n.Id.ToString().Equals("d1ff8ef8-37ef-4bcb-a30e-b410f376fc13")))
        {
            BlockchainNetworks.AddRange(new[] {
                // Mainnets
                new BlockchainNetwork { Id = new Guid("d1ff8ef8-37ef-4bcb-a30e-b410f376fc13"), Name = "Avalanche Network C-Chain", RPCUrl = "https://avalanche-mainnet.infura.io", ChainID = 43114, ExplorerUrl = "https://snowtrace.io/", CurrencySymbol = "AVAX", IsTestNet = false },
                new BlockchainNetwork { Id = new Guid("73c4c4ef-655a-4d25-9ad8-a946bd4c18a3"), Name = "Arbitrum One mainnet", RPCUrl = "https://arb1.arbitrum.io/rpc", ChainID = 42161, ExplorerUrl = "https://arbiscan.io/", CurrencySymbol = "ETH", IsTestNet = false },
                new BlockchainNetwork { Id = new Guid("bf72aefb-eed0-497d-bfe0-c681e2dbcc96"), Name = "NEAR Protocol mainnet", RPCUrl = "https://rpc.mainnet.near.org", ChainID = 0, ExplorerUrl = "https://explorer.near.org/", CurrencySymbol = "NEAR", IsTestNet = false },
                new BlockchainNetwork { Id = new Guid("4efdef2a-a214-41e1-9076-93ef4a0cebe7"), Name = "Fantom Opera mainnet", RPCUrl = "https://rpcapi.fantom.network", ChainID = 250, ExplorerUrl = "https://ftmscan.com/", CurrencySymbol = "FTM", IsTestNet = false },
                new BlockchainNetwork { Id = new Guid("2922a864-66b2-4cf4-8e79-2ddedcc68767"), Name = "Metis Andromeda mainnet", RPCUrl = "https://andromeda.metis.io/?owner=1088", ChainID = 1088, ExplorerUrl = "https://andromeda-explorer.metis.io/", CurrencySymbol = "METIS", IsTestNet = false },
                new BlockchainNetwork { Id = new Guid("41747524-d553-48ed-bf64-8ea621d881c1"), Name = "Aleph Zero mainnet", RPCUrl = "https://alephzero.org/", ChainID = 0, ExplorerUrl = "https://alephzero.subscan.io/", CurrencySymbol = "AZERO", IsTestNet = false },
                // Testnets
                new BlockchainNetwork { Id = new Guid("600026de-e537-4d5f-b732-a585b92e02ee"), Name = "Avalanche Fuji Testnet", RPCUrl = "https://ava-testnet.public.blastapi.io/ext/bc/C/rpc", ChainID = 43113, ExplorerUrl = "https://testnet.snowtrace.io/", CurrencySymbol = "AVAX", IsTestNet = true },
                new BlockchainNetwork { Id = new Guid("b27c6047-e780-4c2b-9465-e78d37964a82"), Name = "Arbitrum Goerli testnet", RPCUrl = "https://arbitrum-goerli.public.blastapi.io", ChainID = 421613, ExplorerUrl = "https://goerli.arbiscan.io/", CurrencySymbol = "ETH", IsTestNet = true },
                new BlockchainNetwork { Id = new Guid("73ea10f9-7ba4-4cbf-becd-7457a175165b"), Name = "NEAR Protocol testnet", RPCUrl = "https://rpc.testnet.near.org", ChainID = 0, ExplorerUrl = "https://explorer.testnet.near.org/", CurrencySymbol = "NEAR", IsTestNet = true },
                new BlockchainNetwork { Id = new Guid("5d34b66b-711c-4f4f-a0db-c5033fedebd0"), Name = "Fantom Testnet", RPCUrl = "https://rpc.testnet.fantom.network", ChainID = 4002, ExplorerUrl = "https://testnet.ftmscan.com/", CurrencySymbol = "FTM", IsTestNet = true },
                new BlockchainNetwork { Id = new Guid("394ec2a5-ae0f-483f-b3bc-05b466a57116"), Name = "Metis Goerli Testnet", RPCUrl = "https://goerli.gateway.metisdevops.link", ChainID = 599, ExplorerUrl = "https://goerli.explorer.metisdevops.link/", CurrencySymbol = "METIS", IsTestNet = true },
                new BlockchainNetwork { Id = new Guid("38f458a9-9f9b-488f-9f06-30db1e9fc884"), Name = "Aleph Zero Testnet", RPCUrl = "https://testnet.alephzero.org/", ChainID = 0, ExplorerUrl = "https://azero.dev/#/explorer", CurrencySymbol = "AZERO", IsTestNet = true },
            });
            SaveChanges();
        }
        // Update AlephZero explorer to use SubScan
        if (BlockchainNetworks.Any(n => n.Id.ToString().Equals("41747524-d553-48ed-bf64-8ea621d881c1") && !n.ExplorerUrl.Equals("https://alephzero.subscan.io/")))
        {
            var azero = BlockchainNetworks.Single(n => n.Id.ToString().Equals("41747524-d553-48ed-bf64-8ea621d881c1"));
            azero.ExplorerUrl = "https://alephzero.subscan.io/";
            BlockchainNetworks.Update(azero);
            SaveChanges();
        }
        // Add default supported exchanges
        if (!Exchanges.Any())
        {
            Exchanges.AddRange(new[] {
                new Exchange { Id = new Guid("253dfc2d-ac8b-413b-9a8b-095a1a7b621b"), ExchangeName = "Binance", Url = "https://www.binance.com/", IsDefi = false, Icon = IconFromAssembly("Hodl.Api.Resources.Images.exchange-binance.png") },
                new Exchange { Id = new Guid("1ee074dc-17c4-495d-8665-66f47a2070d0"), ExchangeName = "Crypto2Cash", Url = "https://www.crypto2cash.com/", IsDefi = false, Icon = IconFromAssembly("Hodl.Api.Resources.Images.exchange-c2c.png") },
                new Exchange { Id = new Guid("150b2348-106a-4ec7-b3ad-c4e185d58d75"), ExchangeName = "Huobi", Url = "https://www.huobi.com/", IsDefi = false, Icon = IconFromAssembly("Hodl.Api.Resources.Images.exchange-huobi.png") },
                new Exchange { Id = new Guid("75ef3d4e-236e-413e-931d-3b2cfa69e4e8"), ExchangeName = "Kucoin", Url = "https://www.kucoin.com/", IsDefi = false, Icon = IconFromAssembly("Hodl.Api.Resources.Images.exchange-kucoin.png") },
                new Exchange { Id = new Guid("9e5e49b1-0d8f-489c-bb8c-1d20312d8b85"), ExchangeName = "Pancakeswap", Url = "https://pancakeswap.finance/", IsDefi = true, Icon = IconFromAssembly("Hodl.Api.Resources.Images.exchange-pancakeswap.png") },
                new Exchange { Id = new Guid("813d7e90-5437-41f3-97d9-ff8723cf77d4"), ExchangeName = "Uniswap", Url = "https://uniswap.org/", IsDefi = true, Icon = IconFromAssembly("Hodl.Api.Resources.Images.exchange-uniswap.png") },
            });
            SaveChanges();
        }
    }

    private static byte[] IconFromAssembly(string resourcePath)
    {
        using var rs = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath);

        if (rs != null)
        {
            using var ms = new MemoryStream();
            rs.CopyTo(ms);

            return ms.ToArray();
        }

        return null;
    }

    public void RevertChanges<T>(IEnumerable<T> items)
    {
        foreach (T item in items)
        {
            EntityEntry entry = Entry(item);
            entry.State = EntityState.Unchanged;
        }
    }
}
