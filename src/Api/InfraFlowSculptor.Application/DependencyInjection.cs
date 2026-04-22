using System.Reflection;
using FluentValidation;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.Common.Services;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.Application.InfrastructureConfig.Diagnostics;
using InfraFlowSculptor.Application.InfrastructureConfig.Diagnostics.Rules;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Application.RoleAssignments.Common;
using InfraFlowSculptor.BicepGeneration;
using InfraFlowSculptor.BicepGeneration.Generators;
using InfraFlowSculptor.PipelineGeneration;
using InfraFlowSculptor.PipelineGeneration.Generators;
using InfraFlowSculptor.PipelineGeneration.Generators.App;
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

        // Behaviors (order matters: Validation runs first, then UoW wraps the handler)
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(UnitOfWorkBehavior<,>));

        // Validators
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Access control services
        services.AddScoped<IInfraConfigAccessService, InfraConfigAccessService>();
        services.AddScoped<IProjectAccessService, ProjectAccessService>();

        // Resource naming
        services.AddScoped<IResourceNameResolver, ResourceNameResolver>();

        // Role assignment domain services
        services.AddScoped<IRoleAssignmentDomainService, RoleAssignmentDomainService>();
        services.AddScoped<IRoleAssignmentImpactAnalyzer, RoleAssignmentImpactAnalyzer>();

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
        services.AddSingleton<IResourceTypeBicepGenerator, LogAnalyticsWorkspaceTypeBicepGenerator>();
        services.AddSingleton<IResourceTypeBicepGenerator, ApplicationInsightsTypeBicepGenerator>();
        services.AddSingleton<IResourceTypeBicepGenerator, CosmosDbTypeBicepGenerator>();
        services.AddSingleton<IResourceTypeBicepGenerator, SqlServerTypeBicepGenerator>();
        services.AddSingleton<IResourceTypeBicepGenerator, SqlDatabaseTypeBicepGenerator>();
        services.AddSingleton<IResourceTypeBicepGenerator, ServiceBusNamespaceTypeBicepGenerator>();
        services.AddSingleton<IResourceTypeBicepGenerator, ContainerRegistryTypeBicepGenerator>();
        services.AddSingleton<BicepGenerationEngine>();

        // Pipeline generation engine
        services.AddSingleton<PipelineGenerationEngine>();
        services.AddSingleton<BootstrapPipelineGenerationEngine>();

        // Application pipeline generation
        services.AddSingleton<IAppPipelineGenerator, ContainerAppPipelineGenerator>();
        services.AddSingleton<IAppPipelineGenerator, WebAppContainerPipelineGenerator>();
        services.AddSingleton<IAppPipelineGenerator, WebAppCodePipelineGenerator>();
        services.AddSingleton<IAppPipelineGenerator, FunctionAppContainerPipelineGenerator>();
        services.AddSingleton<IAppPipelineGenerator, FunctionAppCodePipelineGenerator>();
        services.AddSingleton<AppPipelineGenerationEngine>();

        // Configuration diagnostics
        services.AddScoped<IConfigDiagnosticService, ConfigDiagnosticService>();
        services.AddScoped<IDiagnosticRule, AcrPullDiagnosticRule>();
        services.AddScoped<IDiagnosticRule, KeyVaultAccessDiagnosticRule>();
        services.AddScoped<IDiagnosticRule, NameAvailabilityDiagnosticRule>();

        return services;
    }
}