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
            .AddRepositories();
        
        services.AddMigration<ProjectDbContext>();

        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        return services;
    }

    private static IServiceCollection AddRepositories(
        this IServiceCollection services)
    {
        services.AddScoped<IInfrastructureConfigRepository, InfrastructureConfigRepository>();
        services.AddScoped<IKeyVaultRepository, KeyVaultRepository>();
        services.AddScoped<IResourceGroupRepository, ResourceGroupRepository>();
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
}