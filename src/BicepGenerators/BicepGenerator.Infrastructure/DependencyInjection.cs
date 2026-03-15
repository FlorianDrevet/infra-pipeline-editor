using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using BicepGenerator.Application.Common.Interfaces.Persistence;
using BicepGenerator.Application.Common.Interfaces.Services;
using BicepGenerator.Infrastructure.Persistence.Repositories;
using BicepGenerator.Infrastructure.Services;
using BicepGenerator.Infrastructure.Services.BlobService;
using InfraFlowSculptorDbContext = InfraFlowSculptor.Infrastructure.Persistence.ProjectDbContext;

namespace BicepGenerator.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        ConfigurationManager builderConfiguration)
    {
        var connectionString = builderConfiguration.GetConnectionString("infraDb")
            ?? builderConfiguration.GetConnectionString("ProjectDatabase");

        services
            .AddAuth(builderConfiguration)
            .AddDbContext<InfraFlowSculptorDbContext>(options =>
                options.UseNpgsql(connectionString))
            .AddAzureServices(builderConfiguration)
            .AddBlob(builderConfiguration)
            .AddRepositories();

        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        return services;
    }

    private static IServiceCollection AddRepositories(
        this IServiceCollection services)
    {
        services.AddScoped<IInfrastructureConfigReadRepository, InfrastructureConfigReadRepository>();
        return services;
    }

    private static IServiceCollection AddAzureServices(
        this IServiceCollection services,
        ConfigurationManager builderConfiguration)
    {
        services.AddAzureClients(clientBuilder =>
        {
            string connectionString = builderConfiguration.GetConnectionString("AzureBlobStorageConnectionString")
                ?? string.Empty;
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
