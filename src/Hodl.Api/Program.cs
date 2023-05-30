using Hodl.Api.Services.BackgroundServices;
using Hodl.Api.Services.Identity;
using Hodl.Api.Services.Notifications;
using Hodl.Api.Services.Notifications.Handlers;
using Hodl.Api.Utils.Notifications.Handlers;
using Hodl.ExplorerAPI;
using Hodl.MarketAPI;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.OpenApi.Models;

/**
 * Hodl Trading Desk API 
 * 
 * Version: 1.0
 */

var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddConsole();

var configuration = builder.Configuration;
var services = builder.Services;

// Add services to the container.
services
    .AddConfigurations(configuration)
    .AddLocalization(x => x.ResourcesPath = "Resources")
    .AddDbContext<HodlDbContext>(options => options.UseNpgsql(configuration.GetConnectionString("TradingDeskConnectionString")));

services
    .AddDataProtection()
    .UseCryptographicAlgorithms(new AuthenticatedEncryptorConfiguration
    {
        EncryptionAlgorithm = EncryptionAlgorithm.AES_256_CBC,
        ValidationAlgorithm = ValidationAlgorithm.HMACSHA256
    });

services
    .AddIdentity<AppUser, AppRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true;
        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = 8;
    })
    .AddEntityFrameworkStores<HodlDbContext>()
    .AddTokenProviders();

services
    .AddJwt(configuration.GetSection("JwtOptions"))
    .AddMultiFactorAuthentication()
    .AddCors(options =>
    {
        options.AddDefaultPolicy(builder =>
        {
            builder
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials()
                .SetIsOriginAllowed(origin => true);
        });
        options.AddPolicy("ContentDisposition", builder =>
        {
            builder
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowAnyOrigin()
                .WithExposedHeaders("Content-Disposition");
        });
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
services
    .ConfigureOptions<ConfigureJsonOptions>()
    .AddEndpointsApiExplorer()
    .AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Hodl Trading Desk", Version = "v1.0.0" });
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            BearerFormat = "JWT",
            Name = "Authorization",
            Description = "Please insert JWT with Bearer label"
        });
        c.AddSecurityRequirement(new OpenApiSecurityRequirement()
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference {Type = ReferenceType.SecurityScheme, Id = "Bearer"}
                },
                Array.Empty<string>()
            }
        });
    });

services
    .AddSingleton(provider => new MapperConfiguration(cfg =>
    {
        cfg.CreateMap<DateTime, DateTime>().ConvertUsing(new UtcDateTimeConverter());
        cfg.CreateMap<DateTime?, DateTime?>().ConvertUsing(new UtcDateTimeConverter2());
        // Add all the mappers
        foreach (Type t in ClassHelper.GetLastDescendants(typeof(Profile)))
        {
            cfg.AddProfile((Profile)Activator.CreateInstance(t));
        }
    })
    .CreateMapper());

services
    .AddSingleton<IHttpContextAccessor, HttpContextAccessor>()
    .AddSingleton<IBookingPeriodHelper, MonthlyBookingPeriodHelper>();

services
    .AddScoped<IErrorManager, ErrorManager>()
    .AddScoped<IEmailService, EmailService>()
    .AddScoped<IEmailUserGroupService, EmailUserGroupService>()
    .AddScoped<IJwtTokenManager, JwtTokenManager>()
    .AddScoped<IPasswordHasher, PasswordHasher>()
    .AddScoped<IRoleService, RoleService>()
    .AddScoped<IUserService, UserService>()
    .AddScoped<IUserResolver, UserResolver>()
    .AddScoped<IMultiFactorService, MultiFactorService>()
    .AddScoped<IChangeLogService, ChangeLogService>()
    .AddScoped<IAppConfigService, AppConfigService>()
    .AddScoped<ICryptoCurrencyService, CryptoCurrencyService>()
    .AddScoped<ICurrencyService, CurrencyService>()
    .AddScoped<IFundService, FundService>()
    .AddScoped<IStatisticsService, StatisticsService>()
    .AddScoped<ISandboxService, SandboxService>()
    .AddScoped<IOrderService, OrderService>()
    .AddScoped<IReportService, ReportService>()
    .AddScoped<IExchangeAccountsService, ExchangeAccountsService>()
    .AddScoped<ILayerIdxService, SimpleLayerIdxService>()
    .AddScoped<IExchangeOrderService, ExchangeOrderService>()
    .AddScoped<IDiscordService, DiscordService>()
    .AddScoped<INotificationManager, NotificationManager>()
    .AddScoped<INotificationHandler, SignalRNotificationHandler>()
    .AddScoped<INotificationHandler, EmailNotificationHandler>()
    .AddScoped<INotificationHandler, DiscordNotificationHandler>()
    .AddScoped<IWalletService, WalletService>();

services
    .AddMarketApis(configuration)
    .AddBlockExplorerApis();

// Background services
//#if !DEBUG
services
    .AddSingleton<CryptoCurrencyListingsBackgroundService>()
    .AddHostedService(sp => sp.GetService<CryptoCurrencyListingsBackgroundService>())
    .AddSingleton<CurrencyRatesBackgroundService>()
    .AddHostedService(sp => sp.GetService<CurrencyRatesBackgroundService>())
    .AddSingleton<FundBackgroundService>()
    .AddHostedService(sp => sp.GetService<FundBackgroundService>())
    .AddSingleton<ExchangeBackgroundService>()
    .AddHostedService(sp => sp.GetService<ExchangeBackgroundService>())
    .AddSingleton<WalletBalanceBackgroundService>()
    .AddHostedService(sp => sp.GetService<WalletBalanceBackgroundService>());
//#endif

// Add controllers using JSON converter options to apply strings for enums instead of indexes
services.AddControllers();
// Adding the required service to ASP.NET Core's DI layer
services.AddSignalR();

// Configure the HTTP request pipeline.
var app = builder.Build();

// Migrate Databases
app.ApplyDatabaseMigrations<HodlDbContext>();

// Add localization
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en-Us")
});

app
    .UseRouting()
    // Global CORS policy
    .UseCors()
    // Middleware for authentication
    .UseAuthentication()
    // Middleware for authorization
    .UseAuthorization()
    // Custom middleware
    .UseMiddleware<JwtTokenManagerMiddleware>()
    .UseMiddleware<MultiFactorMiddleware>()
    .UseMiddleware<ErrorHandlingMiddleware>();

// Middlewares for serving the generated JSON document and the Swagger UI
if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

// Setting the mapping for the path & Hub cls type for incoming requests.
app.MapHub<SignalRNotificationHub>("/signalr/notification");


app.Run();
