using System.Net;
using ErrorOr;
using InfraFlowSculptor.Application.Common.GitRouting;
using InfraFlowSculptor.Application.Common.Helpers;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.PipelineGeneration;
using InfraFlowSculptor.PipelineGeneration.Models;

namespace InfraFlowSculptor.Application.Projects.Commands.GenerateProjectBootstrapPipeline;

/// <summary>Handles the <see cref="GenerateProjectBootstrapPipelineCommand"/>.</summary>
public sealed class GenerateProjectBootstrapPipelineCommandHandler(
    IProjectAccessService accessService,
    IProjectRepository projectRepository,
    IInfrastructureConfigReadRepository configReadRepository,
    IProjectBootstrapDefinitionBuilder definitionBuilder,
    BootstrapPipelineGenerationEngine bootstrapEngine,
    IBlobService blobService,
    IRepositoryTargetResolver targetResolver)
    : ICommandHandler<GenerateProjectBootstrapPipelineCommand, GenerateProjectBootstrapPipelineResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<GenerateProjectBootstrapPipelineResult>> Handle(
        GenerateProjectBootstrapPipelineCommand command,
        CancellationToken cancellationToken)
    {
        var authResult = await accessService.VerifyWriteAccessAsync(command.ProjectId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        var project = await projectRepository.GetByIdWithAllAndPipelineVariableGroupsAsync(
            command.ProjectId,
            cancellationToken);
        if (project is null)
            return Errors.Project.NotFoundError(command.ProjectId);

        // Resolve the project-level target (alias "default") for bootstrap artifacts.
        var targetResult = targetResolver.Resolve(project, config: null, ArtifactKind.Bootstrap);
        if (targetResult.IsError)
            return targetResult.Errors;

        var target = targetResult.Value;

        var configs = await configReadRepository.GetAllByProjectIdWithResourcesAsync(
            command.ProjectId.Value,
            cancellationToken);

        if (configs.Count == 0)
            return Errors.Project.NoConfigurationsError();

        var ownerParts = target.Owner.Split('/', 2);
        var organizationName = DecodeUrlSegment(ownerParts[0]);
        var adoProjectName = DecodeUrlSegment(ownerParts.Length > 1 ? ownerParts[1] : ownerParts[0]);

        var isSplit = project.LayoutPreset.Value == LayoutPresetEnum.SplitInfraCode;

        ResolvedRepositoryTarget? appTarget = null;
        if (isSplit)
        {
            var appTargetResult = targetResolver.Resolve(project, config: null, ArtifactKind.BootstrapApplication);
            if (appTargetResult.IsError)
                return appTargetResult.Errors;
            appTarget = appTargetResult.Value;
        }

        var definitions = await definitionBuilder.BuildAsync(
                project,
                configs,
                infraPipelineBasePath: target.PipelineBasePath,
                appPipelineBasePath: isSplit ? appTarget!.PipelineBasePath : target.PipelineBasePath,
                cancellationToken)
            .ConfigureAwait(false);

        var infraPipelines = definitions.InfraPipelines;
        var appPipelines = definitions.AppPipelines;
        var variableGroups = definitions.VariableGroups;
        var bootstrapEnvironments = definitions.Environments;

        var prefix = $"bootstrap/project/{command.ProjectId.Value}/{DateTimeOffset.UtcNow:yyyyMMddHHmmss}";
        var unionFileUris = new Dictionary<string, Uri>(StringComparer.Ordinal);
        var infraFileUris = new Dictionary<string, Uri>(StringComparer.Ordinal);
        var appFileUris = new Dictionary<string, Uri>(StringComparer.Ordinal);

        if (isSplit)
        {
            // Infra bootstrap (FullOwner): infra pipelines + envs + VGs.
            var infraRequest = new BootstrapGenerationRequest
            {
                OrganizationName = organizationName,
                ProjectName = adoProjectName,
                RepositoryName = DecodeUrlSegment(target.RepositoryName),
                DefaultBranch = target.Branch,
                AgentPoolName = project.AgentPoolName,
                Pipelines = infraPipelines,
                Environments = bootstrapEnvironments,
                VariableGroups = variableGroups,
                Mode = BootstrapMode.FullOwner,
            };

            var infraGeneration = bootstrapEngine.Generate(infraRequest);
            foreach (var (path, content) in infraGeneration.Files)
            {
                var uri = await blobService.UploadContentAsync($"{prefix}/infra/{path}", content, "text/plain");
                infraFileUris[path] = uri;
                unionFileUris[$"infra/{path}"] = uri;
            }

            // Application bootstrap (ApplicationOnly): app pipelines + validation of envs/VGs.
            var appRequest = new BootstrapGenerationRequest
            {
                OrganizationName = organizationName,
                ProjectName = adoProjectName,
                RepositoryName = DecodeUrlSegment(appTarget!.RepositoryName),
                DefaultBranch = appTarget.Branch,
                AgentPoolName = project.AgentPoolName,
                Pipelines = appPipelines,
                Environments = bootstrapEnvironments,
                VariableGroups = variableGroups,
                Mode = BootstrapMode.ApplicationOnly,
            };

            var appGeneration = bootstrapEngine.Generate(appRequest);
            foreach (var (path, content) in appGeneration.Files)
            {
                var uri = await blobService.UploadContentAsync($"{prefix}/app/{path}", content, "text/plain");
                appFileUris[path] = uri;
                unionFileUris[$"app/{path}"] = uri;
            }
        }
        else
        {
            // AllInOne / MultiRepo: single bootstrap owns everything (infra + app pipelines).
            var allPipelines = infraPipelines.Concat(appPipelines)
                .DistinctBy(pipeline => new { pipeline.Name, pipeline.YamlPath, pipeline.Folder })
                .ToList();

            var bootstrapRequest = new BootstrapGenerationRequest
            {
                OrganizationName = organizationName,
                ProjectName = adoProjectName,
                RepositoryName = DecodeUrlSegment(target.RepositoryName),
                DefaultBranch = target.Branch,
                AgentPoolName = project.AgentPoolName,
                Pipelines = allPipelines,
                Environments = bootstrapEnvironments,
                VariableGroups = variableGroups,
                Mode = BootstrapMode.FullOwner,
            };

            var generationResult = bootstrapEngine.Generate(bootstrapRequest);

            foreach (var (path, content) in generationResult.Files)
            {
                var uri = await blobService.UploadContentAsync($"{prefix}/{path}", content, "text/plain");
                unionFileUris[path] = uri;
                infraFileUris[path] = uri;
            }
        }

        return new GenerateProjectBootstrapPipelineResult(unionFileUris, infraFileUris, appFileUris);
    }

    private static string DecodeUrlSegment(string value) =>
        WebUtility.UrlDecode(value);
}
