using BicepGenerator.Domain;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace BicepGenerator.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddBicepApplication(this IServiceCollection services)
    {
        // MediatR handlers for Bicep generation (accumulates with main API's registration)
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        // Validators
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        // Domain services — Bicep generators
        services.AddSingleton<IResourceTypeBicepGenerator, StorageAccountTypeBicepGenerator>();
        services.AddSingleton<IResourceTypeBicepGenerator, KeyVaultTypeBicepGenerator>();
        services.AddSingleton<IResourceTypeBicepGenerator, RedisCacheTypeBicepGenerator>();
        services.AddSingleton<IResourceTypeBicepGenerator, AppServicePlanTypeBicepGenerator>();
        services.AddSingleton<IResourceTypeBicepGenerator, WebAppTypeBicepGenerator>();
        services.AddSingleton<BicepGenerationEngine>();

        return services;
    }
}
