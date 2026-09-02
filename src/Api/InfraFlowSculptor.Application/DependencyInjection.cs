using System.Reflection;
using FluentValidation;
using InfraFlowSculptor.Application.Common.GitRouting;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.Common.Services;
using InfraFlowSculptor.Application.AppConfigurations.Commands.AddAppConfigurationKey;
using InfraFlowSculptor.Application.AppSettings.Commands.AddAppSetting;
using InfraFlowSculptor.Application.Imports.Common.Analysis;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.Application.InfrastructureConfig.Diagnostics;
using InfraFlowSculptor.Application.InfrastructureConfig.Diagnostics.Rules;
using InfraFlowSculptor.Application.Projects.Commands.GenerateProjectBootstrapPipeline;
using InfraFlowSculptor.Application.Projects.Commands.PushProjectArtifactsToMultiRepo;
using InfraFlowSculptor.Application.Projects.Commands.PushProjectMultiRepoArtifacts;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Application.Projects.Common.Generation;
using InfraFlowSculptor.Application.Projects.Common.Storage;
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

        // Behaviors (order matters: Logging wraps all, then Validation, then PAT scope, then UoW)
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(PersonalAccessTokenScopeBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(UnitOfWorkBehavior<,>));

        // Validators
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Access control services
        services.AddScoped<IInfraConfigAccessService, InfraConfigAccessService>();
        services.AddScoped<IProjectAccessService, ProjectAccessService>();
        services.AddScoped<IAddAppSettingAdditionService, AddAppSettingAdditionService>();
        services.AddScoped<IAddAppConfigurationKeyAdditionService, AddAppConfigurationKeyAdditionService>();

        // Import preview analysis
        services.AddSingleton<IImportPreviewAnalyzer, ImportPreviewAnalyzer>();

        // Resource naming
        services.AddScoped<IResourceNameResolver, ResourceNameResolver>();
        services.AddScoped<IAppPipelineRequestFactory, AppPipelineRequestFactory>();
        services.AddScoped<IConfigPipelineGenerationService, ConfigPipelineGenerationService>();
        services.AddScoped<IApplicationFolderNameResolver, ApplicationFolderNameResolver>();
        services.AddScoped<IMultiScopeGitPushExecutor, MultiScopeGitPushExecutor>();
        services.AddScoped<IMultiRepoProjectArtifactsPushService, MultiRepoProjectArtifactsPushService>();
        services.AddScoped<IProjectMultiRepoArtifactsPushService, ProjectMultiRepoArtifactsPushService>();
        services.AddScoped<IProjectBootstrapDefinitionBuilder, ProjectBootstrapDefinitionBuilder>();
        services.AddScoped<IProjectPipelineAggregator, ProjectPipelineAggregator>();
        services.AddScoped<IMonoRepoBlobUploadOrchestrator, MonoRepoBlobUploadOrchestrator>();

        // V2 multi-repo Git routing
        services.AddScoped<IRepositoryTargetResolver, RepositoryTargetResolver>();

        // Git repo query helper (shared setup for branch/file query handlers)
        services.AddScoped<IGitRepoQueryHelper, GitRepoQueryHelper>();

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
        services.AddSingleton<IResourceTypeBicepSpecGenerator, DocumentIntelligenceTypeBicepGenerator>();

        // Bicep generation pipeline (Vague 1 — staged decomposition of the engine).
        // Stages are ordered by IBicepGenerationStage.Order at pipeline construction.
        services.AddSingleton<IBicepGenerationStage, IdentityAnalysisStage>();
        services.AddSingleton<IBicepGenerationStage, AppSettingsAnalysisStage>();
        services.AddSingleton<IBicepGenerationStage, ModuleBuildStage>();
        services.AddSingleton<IBicepGenerationStage, IdentityInjectionStage>();
        services.AddSingleton<IBicepGenerationStage, OutputInjectionStage>();
        services.AddSingleton<IBicepGenerationStage, NetworkingResolutionStage>();
        services.AddSingleton<IBicepGenerationStage, PrivateEndpointCompanionStage>();
        services.AddSingleton<IBicepGenerationStage, PublicNetworkAccessStage>();
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
        services.AddScoped<IDiagnosticRule, DockerImageNotSetDiagnosticRule>();
        services.AddScoped<IDiagnosticRule, DockerImageNotValidatedDiagnosticRule>();

        return services;
    }
}
