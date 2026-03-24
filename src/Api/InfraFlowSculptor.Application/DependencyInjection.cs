using System.Reflection;
using FluentValidation;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.BicepGeneration;
using InfraFlowSculptor.BicepGeneration.Generators;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using InfraFlowSculptor.Application.Common.Behaviors;

namespace InfraFlowSculptor.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // CQRS with MediatR
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssemblies(typeof(DependencyInjection).Assembly, Assembly.GetExecutingAssembly()));

        // Behaviors
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // Validators
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Access control services
        services.AddScoped<IInfraConfigAccessService, InfraConfigAccessService>();
        services.AddScoped<IProjectAccessService, ProjectAccessService>();

        // Bicep generation domain services
        services.AddSingleton<IResourceTypeBicepGenerator, StorageAccountTypeBicepGenerator>();
        services.AddSingleton<IResourceTypeBicepGenerator, KeyVaultTypeBicepGenerator>();
        services.AddSingleton<IResourceTypeBicepGenerator, RedisCacheTypeBicepGenerator>();
        services.AddSingleton<IResourceTypeBicepGenerator, AppServicePlanTypeBicepGenerator>();
        services.AddSingleton<IResourceTypeBicepGenerator, WebAppTypeBicepGenerator>();
        services.AddSingleton<IResourceTypeBicepGenerator, FunctionAppTypeBicepGenerator>();
        services.AddSingleton<IResourceTypeBicepGenerator, UserAssignedIdentityTypeBicepGenerator>();
        services.AddSingleton<IResourceTypeBicepGenerator, AppConfigurationTypeBicepGenerator>();
        services.AddSingleton<IResourceTypeBicepGenerator, ContainerAppEnvironmentTypeBicepGenerator>();
        services.AddSingleton<IResourceTypeBicepGenerator, ContainerAppTypeBicepGenerator>();
        services.AddSingleton<BicepGenerationEngine>();

        return services;
    }
}