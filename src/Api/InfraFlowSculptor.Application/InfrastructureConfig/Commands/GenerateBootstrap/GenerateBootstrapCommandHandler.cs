using System.Net;
using ErrorOr;
using InfraFlowSculptor.Application.Common.GitRouting;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.Projects.Commands.GenerateProjectBootstrapPipeline;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.PipelineGeneration;
using InfraFlowSculptor.PipelineGeneration.Models;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.GenerateBootstrap;

/// <summary>
/// Handles the <see cref="GenerateBootstrapCommand"/>.
/// Config-level counterpart of <c>GenerateProjectBootstrapPipelineCommandHandler</c>, used for
/// <c>MultiRepo</c> layouts where each infrastructure configuration owns its own repository.
/// Reuses <see cref="IProjectBootstrapDefinitionBuilder"/> unchanged, passing a single-element
/// configuration list, and always generates in <see cref="BootstrapMode.FullOwner"/> (the
/// config-level bootstrap owns both its infra and app pipeline definitions; <c>SplitInfraCode</c>
/// dual-repository semantics do not apply at this level).
/// </summary>
public sealed class GenerateBootstrapCommandHandler(
    IInfrastructureConfigReadRepository configRepository,
    IProjectRepository projectRepository,
    IProjectBootstrapDefinitionBuilder definitionBuilder,
    BootstrapPipelineGenerationEngine bootstrapEngine,
    IGeneratedArtifactService artifactService,
    IRepositoryTargetResolver targetResolver,
    IInfraConfigAccessService accessService)
    : ICommandHandler<GenerateBootstrapCommand, GenerateBootstrapResult>
{
    public async Task<ErrorOr<GenerateBootstrapResult>> Handle(
        GenerateBootstrapCommand command,
        CancellationToken cancellationToken)
    {
        var configId = new InfrastructureConfigId(command.InfrastructureConfigId);

        var accessResult = await accessService.VerifyWriteAccessAsync(configId, cancellationToken);
        if (accessResult.IsError)
            return accessResult.Errors;

        var domainConfig = accessResult.Value;

        var config = await configRepository.GetByIdWithResourcesAsync(
            command.InfrastructureConfigId, cancellationToken);

        if (config is null)
            return Errors.InfrastructureConfig.NotFoundError(configId);

        var projectId = new ProjectId(config.ProjectId);

        // Load the enriched project snapshot needed by IProjectBootstrapDefinitionBuilder
        // (pipeline variable groups, environment definitions).
        var project = await projectRepository.GetByIdWithAllAndPipelineVariableGroupsAsync(
            projectId, cancellationToken);

        if (project is null)
            return Errors.Project.NotFoundError(projectId);

        // Resolve the infrastructure target for bootstrap artifacts (MultiRepo: config-owned repository).
        var targetResult = targetResolver.Resolve(project, domainConfig, ArtifactKind.Bootstrap);
        if (targetResult.IsError)
            return targetResult.Errors;

        var target = targetResult.Value;
        var isSplit = domainConfig.LayoutMode?.Value == ConfigLayoutModeEnum.SplitInfraCode;
        ResolvedRepositoryTarget? appTarget = null;
        if (isSplit)
        {
            var appTargetResult = targetResolver.Resolve(
                project,
                domainConfig,
                ArtifactKind.BootstrapApplication);
            if (appTargetResult.IsError)
                return appTargetResult.Errors;

            appTarget = appTargetResult.Value;
        }

        // Reuse the project-level definition builder unchanged, scoped to this single configuration.
        var definitions = await definitionBuilder.BuildAsync(
                project,
                configs: [config],
                infraPipelineBasePath: target.PipelineBasePath,
                appPipelineBasePath: isSplit ? appTarget!.PipelineBasePath : target.PipelineBasePath,
                cancellationToken)
            .ConfigureAwait(false);

        var timestamp = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss");
        var fileUris = new Dictionary<string, Uri>();
        var infraFileUris = new Dictionary<string, Uri>();
        var appFileUris = new Dictionary<string, Uri>();

        if (isSplit)
        {
            var infraGeneration = bootstrapEngine.Generate(BuildBootstrapRequest(
                target,
                project,
                definitions.InfraPipelines,
                definitions,
                BootstrapMode.FullOwner));
            await UploadFilesAsync(
                    infraGeneration.Files,
                    "infra",
                    infraFileUris,
                    fileUris,
                    command.InfrastructureConfigId,
                    timestamp)
                .ConfigureAwait(false);

            var appGeneration = bootstrapEngine.Generate(BuildBootstrapRequest(
                appTarget!,
                project,
                definitions.AppPipelines,
                definitions,
                BootstrapMode.ApplicationOnly));
            await UploadFilesAsync(
                    appGeneration.Files,
                    "app",
                    appFileUris,
                    fileUris,
                    command.InfrastructureConfigId,
                    timestamp)
                .ConfigureAwait(false);
        }
        else
        {
            var allPipelines = definitions.InfraPipelines
                .Concat(definitions.AppPipelines)
                .DistinctBy(pipeline => new { pipeline.Name, pipeline.YamlPath, pipeline.Folder })
                .ToList();
            var generationResult = bootstrapEngine.Generate(BuildBootstrapRequest(
                target,
                project,
                allPipelines,
                definitions,
                BootstrapMode.FullOwner));

            await UploadFilesAsync(
                    generationResult.Files,
                    prefix: null,
                    infraFileUris,
                    fileUris,
                    command.InfrastructureConfigId,
                    timestamp)
                .ConfigureAwait(false);
        }

        return new GenerateBootstrapResult(fileUris, infraFileUris, appFileUris);
    }

    private static BootstrapGenerationRequest BuildBootstrapRequest(
        ResolvedRepositoryTarget target,
        Domain.ProjectAggregate.Project project,
        IReadOnlyList<BootstrapPipelineDefinition> pipelines,
        ProjectBootstrapDefinitions definitions,
        BootstrapMode mode)
    {
        var ownerParts = target.Owner.Split('/', 2);
        var organizationName = DecodeUrlSegment(ownerParts[0]);
        var adoProjectName = DecodeUrlSegment(ownerParts.Length > 1 ? ownerParts[1] : ownerParts[0]);

        return new BootstrapGenerationRequest
        {
            OrganizationName = organizationName,
            ProjectName = adoProjectName,
            RepositoryName = DecodeUrlSegment(target.RepositoryName),
            DefaultBranch = target.Branch,
            AgentPoolName = project.AgentPoolName,
            Pipelines = pipelines,
            Environments = definitions.Environments,
            VariableGroups = definitions.VariableGroups,
            ServiceConnections = definitions.ServiceConnections,
            Mode = mode,
        };
    }

    private async Task UploadFilesAsync(
        IReadOnlyDictionary<string, string> files,
        string? prefix,
        IDictionary<string, Uri> scopedFileUris,
        IDictionary<string, Uri> unionFileUris,
        Guid configId,
        string timestamp)
    {
        foreach (var (path, content) in files)
        {
            var storagePath = string.IsNullOrEmpty(prefix) ? path : $"{prefix}/{path}";
            var uri = await artifactService.UploadArtifactAsync(
                    "bootstrap",
                    configId,
                    timestamp,
                    storagePath,
                    content)
                .ConfigureAwait(false);
            scopedFileUris[path] = uri;
            unionFileUris[storagePath] = uri;
        }
    }

    private static string DecodeUrlSegment(string value) =>
        WebUtility.UrlDecode(value);
}
