using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using AzureKeyVaultEmulator.Aspire.Client;
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
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Web;
using Refit;
using System.Net.Http.Headers;
using System.Text.Json;

namespace InfraFlowSculptor.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        ConfigurationManager builderConfiguration,
        IHostEnvironment hostEnvironment)
    {
        services
            .AddAuth(builderConfiguration)
            .AddAzureServices(builderConfiguration)
            .AddBlob(builderConfiguration)
            .AddRepositories()
            .AddGitProviders(builderConfiguration, hostEnvironment);

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
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
        services.AddScoped<IServiceBusNamespaceRepository, ServiceBusNamespaceRepository>();
        services.AddScoped<IContainerRegistryRepository, ContainerRegistryRepository>();
        services.AddScoped<IEventHubNamespaceRepository, EventHubNamespaceRepository>();
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
        services.AddSingleton<IGeneratedArtifactService, GeneratedArtifactService>();

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
        this IServiceCollection services,
        ConfigurationManager builderConfiguration,
        IHostEnvironment hostEnvironment)
    {
        if (hostEnvironment.IsDevelopment())
        {
            var vaultUri = builderConfiguration.GetConnectionString("keyvault")
                           ?? throw new InvalidOperationException(
                               "Connection string 'keyvault' is required. Configure it via Aspire or appsettings.");
            services.AddAzureKeyVaultEmulator(vaultUri);
        }
        else
        {
            var credential = new DefaultAzureCredential();

            services.AddSingleton(sp =>
            {
                var vaultUri = builderConfiguration.GetConnectionString("keyvault")
                               ?? throw new InvalidOperationException(
                                   "Connection string 'keyvault' is required. Configure it via Aspire or appsettings.");
                return new SecretClient(new Uri(vaultUri), credential);
            });
        }

        services.AddSingleton<IKeyVaultSecretClient, KeyVaultSecretClient>();

        services.AddRefitClient<IGitHubTreeApi>(new RefitSettings
            {
                ContentSerializer = new SystemTextJsonContentSerializer(
                    new JsonSerializerOptions(JsonSerializerDefaults.Web)),
            })
            .ConfigureHttpClient(c =>
            {
                c.BaseAddress = new Uri("https://api.github.com");
                c.DefaultRequestHeaders.UserAgent.Add(
                    new ProductInfoHeaderValue("InfraFlowSculptor", "1.0"));
            });

        services.AddTransient<GitHubGitProviderService>();
        services.AddTransient<AzureDevOpsGitProviderService>();
        services.AddTransient<IGitProviderFactory, GitProviderFactory>();
        services.AddHttpClient();

        return services;
    }
}