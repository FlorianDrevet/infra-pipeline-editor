using ErrorOr;
using InfraFlowSculptor.Application.Common.GitRouting;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Projects.Common.Generation;
using InfraFlowSculptor.Application.Projects.Common.Storage;
using InfraFlowSculptor.Domain.Common.Errors;

namespace InfraFlowSculptor.Application.Projects.Commands.GenerateProjectPipeline;

/// <summary>Handles the <see cref="GenerateProjectPipelineCommand"/>.</summary>
public sealed class GenerateProjectPipelineCommandHandler(
    IProjectAccessService accessService,
    IProjectRepository projectRepository,
    IInfrastructureConfigReadRepository configReadRepository,
    IRepositoryTargetResolver targetResolver,
    IProjectPipelineAggregator projectPipelineAggregator,
    IMonoRepoBlobUploadOrchestrator blobUploadOrchestrator)
    : ICommandHandler<GenerateProjectPipelineCommand, GenerateProjectPipelineResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<GenerateProjectPipelineResult>> Handle(
        GenerateProjectPipelineCommand command,
        CancellationToken cancellationToken)
    {
        // 1. Verify write access
        var authResult = await accessService.VerifyWriteAccessAsync(command.ProjectId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        var projectForGate = authResult.Value;

        // 2. Load all configurations for this project
        var configs = await configReadRepository.GetAllByProjectIdWithResourcesAsync(
            command.ProjectId.Value, cancellationToken);

        if (configs.Count == 0)
            return Errors.Project.NoConfigurationsError();

        // Reject project-level generate-all for heterogeneous multi-repo topologies.
        if (!projectForGate.CanGenerateAllFromProjectLevel())
            return Errors.GitRouting.AmbiguousProjectLevelGeneration;

        // 3. Load the enriched project snapshot needed for repository routing and variable groups.
        var project = await projectRepository.GetByIdWithAllAndPipelineVariableGroupsAsync(
            command.ProjectId, cancellationToken);
        if (project is null)
            return Errors.Project.NotFoundError(command.ProjectId);

        // Resolve the project-level target to determine base paths within the repo.
        // Heterogeneous multi-repo projects will simply fall back to null paths here — the per-config
        // push handlers are responsible for enforcing the routing at push time.
        string? bicepBasePath = null;
        string? pipelineBasePath = null;
        var targetResult = targetResolver.Resolve(project, config: null, ArtifactKind.Pipeline);
        if (!targetResult.IsError)
        {
            bicepBasePath = targetResult.Value.BasePath;
            pipelineBasePath = targetResult.Value.PipelineBasePath;
        }

        // 4. Generate and assemble the mono-repo pipeline output.
        var aggregationResult = await projectPipelineAggregator.GenerateAsync(
                configs,
                project.PipelineVariableGroups,
                project.AgentPoolName,
                bicepBasePath,
                pipelineBasePath,
                cancellationToken)
            .ConfigureAwait(false);
        if (aggregationResult.IsError)
            return aggregationResult.Errors;

        // 5. Upload the assembled mono-repo output to blob storage.
        var prefix = $"pipeline/project/{command.ProjectId.Value}/{DateTimeOffset.UtcNow:yyyyMMddHHmmss}";
        var uploadResult = await blobUploadOrchestrator.UploadPipelineAsync(
                prefix,
                aggregationResult.Value,
                cancellationToken)
            .ConfigureAwait(false);

        return new GenerateProjectPipelineResult(
            CommonFileUris: uploadResult.CommonFileUris,
            ConfigFileUris: uploadResult.ConfigFileUris,
            InfraCommonFileUris: uploadResult.InfraCommonFileUris,
            AppCommonFileUris: uploadResult.AppCommonFileUris,
            InfraConfigFileUris: uploadResult.InfraConfigFileUris,
            AppConfigFileUris: uploadResult.AppConfigFileUris);
    }
}
