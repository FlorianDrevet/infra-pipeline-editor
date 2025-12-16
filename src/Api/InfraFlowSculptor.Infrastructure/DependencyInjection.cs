using System.Text.Json;
using InfraFlowSculptor.Application.Common.Clients;
using InfraFlowSculptor.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Infrastructure.Extensions;
using InfraFlowSculptor.Infrastructure.Factories;
using InfraFlowSculptor.Infrastructure.Options;
using InfraFlowSculptor.Infrastructure.Persistence;
using InfraFlowSculptor.Infrastructure.Persistence.Repositories;
using InfraFlowSculptor.Infrastructure.Services;
using InfraFlowSculptor.Infrastructure.Services.Authentication;
using Microsoft.Identity.Web;
using Refit;

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
            .AddBicepGeneratorApi(builderConfiguration)
            .AddRepositories();
        
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
        services.AddScoped<IKeyVaultRepository, KeyVaultRepository>();
        services.AddScoped<IResourceGroupRepository, ResourceGroupRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

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

    private static IServiceCollection AddAuth(
        this IServiceCollection services,
        ConfigurationManager builderConfiguration)
    {
        services.AddAuthentication(defaultScheme: JwtBearerDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApi(builderConfiguration.GetSection("AzureAd"));
        
        return services;
    }
    
    private static IServiceCollection AddBicepGeneratorApi(
        this IServiceCollection services,
        ConfigurationManager builderConfiguration)
    {
        services.AddMemoryCache();
        
        services.AddOptions<AzureAdOptions>()
            .Bind(builderConfiguration.GetSection(AzureAdOptions.SectionName))
            .ValidateDataAnnotations();
        
        services.AddSingleton<IBearerTokenService, BearerTokenService>();
        services.AddSingleton<IAzureAdService, AzureAdService>();

        var options = new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
        var jsonSerializerOptions = new SystemTextJsonContentSerializer(options);
        RefitSettings refitSettings = new()
        {
            // Tell Refit to use AuthBearerTokenFactory.GetBearerTokenAsync() whenever it needs an OAuth token string.
            // This is a lambda, so it won't be called until a request is made.
            AuthorizationHeaderValueGetter = (_, cancellationToken) => AuthBearerTokenFactory.GetBearerTokenAsync(cancellationToken),
            ContentSerializer = jsonSerializerOptions
        };

        services.AddRefitClient<IGenerateBicepClient>(refitSettings)
            .ConfigureHttpClient(client =>
            {
                var bicepGeneratorApiSettings = builderConfiguration
                    .GetSection(BicepGeneratorOptions.SectionName)
                    .Get<BicepGeneratorOptions>() ?? throw new ArgumentNullException(nameof(BicepGeneratorOptions));
                
                client.BaseAddress = bicepGeneratorApiSettings.BaseUri;
            });
        return services;
    }
}