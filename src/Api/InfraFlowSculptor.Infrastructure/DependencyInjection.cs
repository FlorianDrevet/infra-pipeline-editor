using Azure.Identity;
using InfraFlowSculptor.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Infrastructure.Extensions;
using InfraFlowSculptor.Infrastructure.Persistence;
using InfraFlowSculptor.Infrastructure.Persistence.Repositories;
using InfraFlowSculptor.Infrastructure.Services;
using InfraFlowSculptor.Infrastructure.Services.BlobService;
using InfraFlowSculptor.Infrastructure.Services.GitProviders;
using InfraFlowSculptor.Infrastructure.Services.KeyVault;
using Microsoft.Identity.Web;

namespace InfraFlowSculptor.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        ConfigurationManager builderConfiguration)
    {
        services
            .AddAuth(builderConfiguration)
            .AddAzureServices(builderConfiguration)
            .AddBlob(builderConfiguration)
            .AddRepositories()
            .AddGitProviders();
        
        services.AddMigration<ProjectDbContext>();

        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddHttpContextAccessor();

        return services;
    }

    private static IServiceCollection AddRepositories(
        this IServiceCollection services)
    {
        services.AddScoped<IInfrastructureConfigRepository, InfrastructureConfigRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IKeyVaultRepository, KeyVaultRepository>();
        services.AddScoped<IRedisCacheRepository, RedisCacheRepository>();
        services.AddScoped<IResourceGroupRepository, ResourceGroupRepository>();
        services.AddScoped<IStorageAccountRepository, StorageAccountRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAzureResourceRepository, AzureResourceBaseRepository>();
        services.AddScoped<IAppServicePlanRepository, AppServicePlanRepository>();
        services.AddScoped<IWebAppRepository, WebAppRepository>();
        services.AddScoped<IFunctionAppRepository, FunctionAppRepository>();
        services.AddScoped<IUserAssignedIdentityRepository, UserAssignedIdentityRepository>();
        services.AddScoped<IAppConfigurationRepository, AppConfigurationRepository>();
        services.AddScoped<IContainerAppEnvironmentRepository, ContainerAppEnvironmentRepository>();
        services.AddScoped<IContainerAppRepository, ContainerAppRepository>();
        services.AddScoped<ILogAnalyticsWorkspaceRepository, LogAnalyticsWorkspaceRepository>();
        services.AddScoped<IApplicationInsightsRepository, ApplicationInsightsRepository>();
        services.AddScoped<ICosmosDbRepository, CosmosDbRepository>();
        services.AddScoped<ISqlServerRepository, SqlServerRepository>();
        services.AddScoped<ISqlDatabaseRepository, SqlDatabaseRepository>();
        services.AddScoped<IInfrastructureConfigReadRepository, InfrastructureConfigReadRepository>();

        return services;
    }

    private static IServiceCollection AddAzureServices(
        this IServiceCollection services,
        ConfigurationManager builderConfiguration)
    {
        services.AddAzureClients(clientBuilder =>
        {
            // Blob Service
            string connectionString = builderConfiguration.GetConnectionString("AzureBlobStorageConnectionString") ??
                                      string.Empty;
            clientBuilder.AddBlobServiceClient(connectionString);
        });
        return services;
    }

    private static IServiceCollection AddBlob(
        this IServiceCollection services,
        ConfigurationManager builderConfiguration)
    {
        var blobSettings = new BlobSettings();
        builderConfiguration.Bind(BlobSettings.SectionName, blobSettings);
        services.AddSingleton(Options.Create(blobSettings));

        services.AddSingleton<IBlobService, BlobService>();

        return services;
    }

    private static IServiceCollection AddAuth(
        this IServiceCollection services,
        ConfigurationManager builderConfiguration)
    {
        services.AddAuthentication(defaultScheme: JwtBearerDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApi(builderConfiguration.GetSection("AzureAd"));
        
        return services;
    }

    private static IServiceCollection AddGitProviders(
        this IServiceCollection services)
    {
        var credential = new DefaultAzureCredential();

        services.AddSingleton(sp =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            var vaultUri = configuration.GetConnectionString("keyvault")
                           ?? throw new InvalidOperationException(
                               "Connection string 'keyvault' is required. Configure it via Aspire or appsettings.");
            return new Azure.Security.KeyVault.Secrets.SecretClient(new Uri(vaultUri), credential);
        });

        services.AddSingleton<IKeyVaultSecretClient, KeyVaultSecretClient>();
        services.AddSingleton<GitHubGitProviderService>();
        services.AddSingleton<AzureDevOpsGitProviderService>();
        services.AddSingleton<IGitProviderFactory, GitProviderFactory>();
        services.AddHttpClient();

        return services;
    }
}