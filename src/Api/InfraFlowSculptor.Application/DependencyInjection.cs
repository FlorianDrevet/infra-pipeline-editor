using System.Reflection;
using FluentValidation;
using InfraFlowSculptor.Application.Common.GitRouting;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.Common.Services;
using InfraFlowSculptor.Application.Imports.Common;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.Application.InfrastructureConfig.Diagnostics;
using InfraFlowSculptor.Application.InfrastructureConfig.Diagnostics.Rules;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Application.RoleAssignments.Common;
using InfraFlowSculptor.BicepGeneration;
using InfraFlowSculptor.BicepGeneration.Generators;
using InfraFlowSculptor.BicepGeneration.Pipeline;
using InfraFlowSculptor.BicepGeneration.Pipeline.Stages;
using InfraFlowSculptor.PipelineGeneration;
using InfraFlowSculptor.PipelineGeneration.Bootstrap;
using InfraFlowSculptor.PipelineGeneration.Bootstrap.Stages;
using InfraFlowSculptor.PipelineGeneration.Generators;
using InfraFlowSculptor.PipelineGeneration.Generators.App;
using InfraFlowSculptor.PipelineGeneration.Infra;
using InfraFlowSculptor.PipelineGeneration.Infra.Stages;
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

        // Import preview analysis
        services.AddSingleton<IImportPreviewAnalyzer, ImportPreviewAnalyzer>();

        // Resource naming
        services.AddScoped<IResourceNameResolver, ResourceNameResolver>();

        // V2 multi-repo Git routing
        services.AddScoped<IRepositoryTargetResolver, RepositoryTargetResolver>();

        // Role assignment domain services
        services.AddScoped<IRoleAssignmentDomainService, RoleAssignmentDomainService>();
        services.AddScoped<IRoleAssignmentImpactAnalyzer, RoleAssignmentImpactAnalyzer>();

        // Bicep generation domain services
        services.AddSingleton<IResourceTypeBicepSpecGenerator, StorageAccountTypeBicepGenerator>();
        services.AddSingleton<IResourceTypeBicepSpecGenerator, KeyVaultTypeBicepGenerator>();
        services.AddSingleton<IResourceTypeBicepSpecGenerator, RedisCacheTypeBicepGenerator>();
        services.AddSingleton<IResourceTypeBicepSpecGenerator, AppServicePlanTypeBicepGenerator>();
        services.AddSingleton<IResourceTypeBicepSpecGenerator, WebAppTypeBicepGenerator>();
        services.AddSingleton<IResourceTypeBicepSpecGenerator, FunctionAppTypeBicepGenerator>();
        services.AddSingleton<IResourceTypeBicepSpecGenerator, UserAssignedIdentityTypeBicepGenerator>();
        services.AddSingleton<IResourceTypeBicepSpecGenerator, AppConfigurationTypeBicepGenerator>();
        services.AddSingleton<IResourceTypeBicepSpecGenerator, ContainerAppEnvironmentTypeBicepGenerator>();
        services.AddSingleton<IResourceTypeBicepSpecGenerator, ContainerAppTypeBicepGenerator>();
        services.AddSingleton<IResourceTypeBicepSpecGenerator, LogAnalyticsWorkspaceTypeBicepGenerator>();
        services.AddSingleton<IResourceTypeBicepSpecGenerator, ApplicationInsightsTypeBicepGenerator>();
        services.AddSingleton<IResourceTypeBicepSpecGenerator, CosmosDbTypeBicepGenerator>();
        services.AddSingleton<IResourceTypeBicepSpecGenerator, SqlServerTypeBicepGenerator>();
        services.AddSingleton<IResourceTypeBicepSpecGenerator, SqlDatabaseTypeBicepGenerator>();
        services.AddSingleton<IResourceTypeBicepSpecGenerator, ServiceBusNamespaceTypeBicepGenerator>();
        services.AddSingleton<IResourceTypeBicepSpecGenerator, ContainerRegistryTypeBicepGenerator>();
        services.AddSingleton<IResourceTypeBicepSpecGenerator, EventHubNamespaceTypeBicepGenerator>();

        // Bicep generation pipeline (Vague 1 — staged decomposition of the engine).
        // Stages are ordered by IBicepGenerationStage.Order at pipeline construction.
        services.AddSingleton<IBicepGenerationStage, IdentityAnalysisStage>();
        services.AddSingleton<IBicepGenerationStage, AppSettingsAnalysisStage>();
        services.AddSingleton<IBicepGenerationStage, ModuleBuildStage>();
        services.AddSingleton<IBicepGenerationStage, IdentityInjectionStage>();
        services.AddSingleton<IBicepGenerationStage, OutputInjectionStage>();
        services.AddSingleton<IBicepGenerationStage, AppSettingsInjectionStage>();
        services.AddSingleton<IBicepGenerationStage, TagsInjectionStage>();
        services.AddSingleton<IBicepGenerationStage, ParentReferenceResolutionStage>();
        services.AddSingleton<IBicepGenerationStage, SpecEmissionStage>();
        services.AddSingleton<IBicepGenerationStage, AssemblyStage>();
        services.AddSingleton<IBicepGenerationStage, IrOutputPruningStage>();
        services.AddSingleton<BicepGenerationPipeline>();
        services.AddSingleton<BicepGenerationEngine>();

        // Infrastructure pipeline stages
        services.AddSingleton<IInfraPipelineStage, CiPipelineStage>();
        services.AddSingleton<IInfraPipelineStage, PrPipelineStage>();
        services.AddSingleton<IInfraPipelineStage, ReleasePipelineStage>();
        services.AddSingleton<IInfraPipelineStage, ConfigVarsStage>();
        services.AddSingleton<IInfraPipelineStage, EnvironmentVarsStage>();
        services.AddSingleton<InfraPipeline>();
        services.AddSingleton<PipelineGenerationEngine>();

        // Bootstrap pipeline stages
        services.AddSingleton<IBootstrapPipelineStage, HeaderEmissionStage>();
        services.AddSingleton<IBootstrapPipelineStage, ValidateSharedResourcesJobStage>();
        services.AddSingleton<IBootstrapPipelineStage, PipelineProvisionJobStage>();
        services.AddSingleton<IBootstrapPipelineStage, EnvironmentProvisionJobStage>();
        services.AddSingleton<IBootstrapPipelineStage, VariableGroupProvisionJobStage>();
        services.AddSingleton<IBootstrapPipelineStage, NoOpFallbackStage>();
        services.AddSingleton<BootstrapPipeline>();
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