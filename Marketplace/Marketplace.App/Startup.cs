using System.Collections.ObjectModel;
using System.Text.Json.Nodes;
using Marketplace.Bot.Abstractions;
using Marketplace.Bot.Telegram.Implementations;
using Marketplace.Core.Abstractions.Data;
using Marketplace.Core.Abstractions.Services;
using Marketplace.Core.Bot.Abstractions;
using Marketplace.Core.Bot.Implementations;
using Marketplace.Core.Bot.Implementations.Middlewares;
using Marketplace.Core.Bot.Logic.Abstractions;
using Marketplace.Core.Bot.Logic.Controllers;
using Marketplace.Core.Bot.Logic.Implementations;
using Marketplace.Core.Bot.Logic.Middlewares;
using Marketplace.Core.Bot.Logic.Models;
using Marketplace.Core.Implementations.Services;
using Marketplace.Core.Models.Settings;
using Marketplace.Core.Models.UserStates;
using Marketplace.EntityFrameworkCore;
using Marketplace.EntityFrameworkCore.Implementations;
using Marketplace.InMemoryCache.Services;
using Marketplace.Resources.Implementations;
using Marketplace.Resources.Models;
using Marketplace.Workers.Implementations.Services;
using Marketplace.Workers.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot;

namespace Marketplace.App;

public static class Startup
{
    private const string CatalogRefreshWorkerKey = "CatalogRefreshWorker";
    private const string SubscriptionRefreshWorkerKey = "SubscriptionRefreshWorkerKey";
    
    public static void ConfigureBusinessServices(this IServiceCollection services, IConfiguration config)
    {
        AddCoreServices(services);
        AddEfCore(services, config);
        AddTelegram(services, config);
        AddBotServices(services);
        AddResources(services, config);
        AddConfigs(services, config);
        AddWorkers(services);
    }

    private static void AddCoreServices(IServiceCollection services)
    {
        services.AddScoped<ICatalogRefreshService, CatalogRefreshService>();
        services.AddScoped<ICatalogService, InMemoryCatalogService>();
        services.AddScoped<IComplaintService, ComplaintService>();
        services.AddScoped<ICurrencyViewFactory, CurrencyViewFactory>();
        services.AddScoped<ILikeService, LikeService>();
        services.AddScoped<IProjectCategoryService, ProjectCategoryService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IProjectTagService, ProjectTagService>();
        services.AddScoped<IPromocodeService, PromocodeService>();
        services.AddScoped<IReferralInvitationService, ReferralInvitationService>();
        services.AddScoped<IStringParser, StringParser>();
        services.AddScoped<ISubscriptionRefreshService, SubscriptionRefreshService>();
        services.AddScoped<ISubscriptionService, SubscriptionService>();
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IUserStateService, InMemoryUserStateService>();
    }

    private static void AddEfCore(IServiceCollection services, IConfiguration config)
    {
        const string connectionString = "Default";
        services.AddDbContext<AppDbContext>(
            options => options.UseNpgsql(config.GetConnectionString(connectionString)));
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
    }

    private static void AddTelegram(IServiceCollection services, IConfiguration config)
    {
        const string botSectionKey = "TelegramBot";
        var botConfig = config.GetSection(botSectionKey).Get<TelegramBotClientSettings>()
                        ?? throw new InvalidOperationException("Bot settings is missing");
        
        var botOptions = new TelegramBotClientOptions(botConfig.Token, botConfig.BaseUrl, botConfig.UseTestEnvironment);
        var telegramBotClient = new TelegramBotClient(botOptions);
        services.AddSingleton(telegramBotClient);
    }

    private static void AddBotServices(IServiceCollection services)
    {
        services.AddScoped<IBotClient, TgBotClient>();
        services.AddScoped<IExtendedBotClient, ExtendedBotClient>();
        services.AddSingleton<IControllerRegistryBuilder>(CreateControllerRegistry);
        services.AddScoped<IControllerFactory>(sp =>
        {
            var registryBuilder = sp.GetRequiredService<IControllerRegistryBuilder>();
            return registryBuilder.Factory;
        });
        services.AddSingleton<IUpdatePipelineBuilder>(_ => CreateMiddlewarePipeline());
        services.AddScoped<IUpdatePipelineInvocator, UpdatePipelineInvocator>();
        services.AddTransient<IKeyboardFactory, KeyboardFactory>();
    }

    private static ControllerRegistryBuilder CreateControllerRegistry(IServiceProvider serviceProvider)
    {
        var builder = new ControllerRegistryBuilder();
        
        // Welcome controller
        builder.RegisterControllerFactoryMethod<DefaultUserState>(
            ctx => new WelcomeController(
                ctx,
                serviceProvider.GetRequiredService<IAssetProvider>(),
                serviceProvider.GetRequiredService<IExtendedBotClient>(),
                serviceProvider.GetRequiredService<IUserStateService>(),
                serviceProvider.GetRequiredService<IControllerFactory>()));
        
        // Project management controller
        builder.RegisterControllerFactoryMethod<ProjectManagementState>(
            ctx => new ProjectManagementController(
                ctx,
                serviceProvider.GetRequiredService<IControllerFactory>(),
                serviceProvider.GetRequiredService<IUserStateService>(),
                serviceProvider.GetRequiredService<IExtendedBotClient>(),
                serviceProvider.GetRequiredService<IAssetProvider>(),
                serviceProvider.GetRequiredService<IProjectService>(),
                serviceProvider.GetRequiredService<IStringParser>()));
        
        // Project creation controller
        builder.RegisterControllerFactoryMethod<ProjectCreationUserState>(
            ctx => new ProjectCreationController(
                ctx,
                serviceProvider.GetRequiredService<IControllerFactory>(),
                serviceProvider.GetRequiredService<IUserStateService>(),
                serviceProvider.GetRequiredService<IExtendedBotClient>(),
                serviceProvider.GetRequiredService<IAssetProvider>(),
                serviceProvider.GetRequiredService<IProjectCategoryService>(),
                serviceProvider.GetRequiredService<IProjectService>(),
                serviceProvider.GetRequiredService<IProjectTagService>(),
                serviceProvider.GetRequiredService<IKeyboardFactory>(),
                serviceProvider.GetRequiredService<ILogger<ProjectCreationController>>()));
        
        // Profile controller
        builder.RegisterControllerFactoryMethod<MyProfileUserState>(
            ctx => new ProfileController(
                ctx,
                serviceProvider.GetRequiredService<IControllerFactory>(),
                serviceProvider.GetRequiredService<IUserStateService>(),
                serviceProvider.GetRequiredService<IExtendedBotClient>(),
                serviceProvider.GetRequiredService<IAssetProvider>(),
                serviceProvider));
        
        // EditUserDataController
        builder.RegisterControllerFactoryMethod<EditUserDataState>(
            ctx => new EditUserDataController(
                ctx,
                serviceProvider.GetRequiredService<IControllerFactory>(),
                serviceProvider.GetRequiredService<IUserStateService>(),
                serviceProvider.GetRequiredService<IExtendedBotClient>(),
                serviceProvider.GetRequiredService<IAssetProvider>(),
                serviceProvider.GetRequiredService<IUserService>()));
        
        // Catalog filter controller
        builder.RegisterControllerFactoryMethod<ProjectSearchUserState>(
            ctx => new CatalogFilterController(
                ctx,
                serviceProvider.GetRequiredService<IControllerFactory>(),
                serviceProvider.GetRequiredService<IUserStateService>(),
                serviceProvider.GetRequiredService<IExtendedBotClient>(),
                serviceProvider.GetRequiredService<IAssetProvider>(),
                serviceProvider.GetRequiredService<IProjectCategoryService>(),
                serviceProvider.GetRequiredService<IProjectTagService>(),
                serviceProvider.GetRequiredService<IKeyboardFactory>()));
        
        // Catalog controller
        builder.RegisterControllerFactoryMethod<ViewCatalogUserState>(
            ctx => new CatalogController(
                ctx,
                serviceProvider.GetRequiredService<IControllerFactory>(),
                serviceProvider.GetRequiredService<IUserStateService>(),
                serviceProvider.GetRequiredService<IExtendedBotClient>(),
                serviceProvider.GetRequiredService<ICatalogService>(),
                serviceProvider.GetRequiredService<ILikeService>(),
                serviceProvider.GetRequiredService<IAssetProvider>()));
        
        return builder;
    }

    private static UpdatePipelineBuilder CreateMiddlewarePipeline()
    {
        var builder = new UpdatePipelineBuilder();
        
        builder.AddMiddleware<ErrorNotifierMiddleware>();
        builder.AddMiddleware<TransactionHandlingMiddleware>();
        builder.AddMiddleware<RegistrationMiddleware>();
        builder.AddMiddleware<StateResetMiddleware>();
        builder.AddMiddleware<SubscriptionMiddleware>();
        builder.AddMiddleware<ControllerDispatcherMiddleware>();
        
        return builder;
    }
    
    private static void AddResources(IServiceCollection services, IConfiguration config)
    {
        const string assetsFilePathKey = "AssetsFile";
        const string invoicesKey = "Invoices";
        const string textsKey = "Replicas";
        
        var assetsPath = config.GetValue<string>(assetsFilePathKey)
                         ?? throw new InvalidOperationException("Assets file path is missing");
        var assetsJson = File.ReadAllText(assetsPath);
        
        var jsonRoot = JsonNode.Parse(assetsJson) ?? throw new FormatException("Invalid assets json");
        var invoices = jsonRoot[invoicesKey]?.GetValue<Dictionary<string, InvoiceResource>>()
            ?? throw new FormatException("Invalid assets json - invoices");
        var replicas = jsonRoot[textsKey]?.GetValue<Dictionary<string, TextResource>>()
            ?? throw new FormatException("Invalid assets json - replicas");
        
        var assetProvider = new AssetProvider(
            new ReadOnlyDictionary<string, TextResource>(replicas),
            new Dictionary<string, InvoiceResource>(invoices));
        
        services.AddSingleton<IAssetProvider>(assetProvider);
    }

    private static void AddConfigs(IServiceCollection services, IConfiguration config)
    {
        // Catalog refresh worker settings
        const string catalogRefreshSettingsKey = "Workers:CatalogRefresh";
        var catalogRefreshSettings = config.GetValue<WorkerSettings>(catalogRefreshSettingsKey)
            ?? throw new InvalidOperationException("Catalog refresh worker settings is missing");
        services.AddKeyedSingleton(CatalogRefreshWorkerKey, catalogRefreshSettings);
        
        // Subscription refresh worker settings
        const string subscriptionRefreshSettingsKey = "Workers:SubscriptionRefresh";
        var subscriptionRefreshSettings = config.GetValue<WorkerSettings>(subscriptionRefreshSettingsKey)
            ?? throw new InvalidOperationException("Subscription refresh worker settings is missing");
        services.AddKeyedSingleton(SubscriptionRefreshWorkerKey, subscriptionRefreshSettings);
        
        // App settings
        const string appSettingsKey = "Settings:App";
        var appSettings = config.GetValue<AppSettings>(appSettingsKey)
            ?? throw new InvalidOperationException("App settings is missing");
        services.AddSingleton(appSettings);
        
        // Referral invitation settings
        const string referralInvitationSettingsKey = "Settings:ReferralInvitation";
        var refSettings = config.GetValue<ReferralInvitationSettings>(referralInvitationSettingsKey)
            ?? throw new InvalidOperationException("Referral invitation settings is missing");
        services.AddSingleton(refSettings);
        
        // Subscription settings
        const string subscriptionSettingsKey = "Settings:Subscription";
        var subscriptionSettings = config.GetValue<SubscriptionSettings>(subscriptionSettingsKey)
            ?? throw new InvalidOperationException("Subscription settings is missing");
        services.AddSingleton(subscriptionSettings);
        
        // Invoice settings
        const string invoiceSettingsKey = "Settings:Invoice";
        var invoiceSettings = config.GetValue<InvoiceSettings>(invoiceSettingsKey)
            ?? throw new InvalidOperationException("Invoice settings is missing");
        services.AddSingleton(invoiceSettings);
    }

    private static void AddWorkers(IServiceCollection services)
    {
        services.AddHostedService<CatalogRefresherWorker>(sp =>
        {
            var settings = sp.GetRequiredKeyedService<WorkerSettings>(CatalogRefreshWorkerKey);
            return new CatalogRefresherWorker(
                sp, settings, sp.GetRequiredService<ILogger<CatalogRefresherWorker>>());
        });
        services.AddHostedService<SubscriptionRefreshWorker>(sp =>
        {
            var settings = sp.GetRequiredKeyedService<WorkerSettings>(SubscriptionRefreshWorkerKey);
            return new SubscriptionRefreshWorker(
                sp, settings, sp.GetRequiredService<ILogger<SubscriptionRefreshWorker>>());
        });
    }
}
