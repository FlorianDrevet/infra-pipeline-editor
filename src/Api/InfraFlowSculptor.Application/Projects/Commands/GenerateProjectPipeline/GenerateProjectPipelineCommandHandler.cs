using ErrorOr;
using InfraFlowSculptor.Application.Common.Generation;
using InfraFlowSculptor.Application.Common.GitRouting;
using InfraFlowSculptor.Application.Common.Helpers;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.GenerationCore.Models;
using InfraFlowSculptor.PipelineGeneration;
using InfraFlowSculptor.PipelineGeneration.Models;
using MediatR;

namespace InfraFlowSculptor.Application.Projects.Commands.GenerateProjectPipeline;

/// <summary>Handles the <see cref="GenerateProjectPipelineCommand"/>.</summary>
public sealed class GenerateProjectPipelineCommandHandler(
    IProjectAccessService accessService,
    IProjectRepository projectRepository,
    IInfrastructureConfigReadRepository configReadRepository,
    PipelineGenerationEngine pipelineGenerationEngine,
    IConfigPipelineGenerationService configPipelineGenerationService,
    IBlobService blobService,
    IRepositoryTargetResolver targetResolver)
    : ICommandHandler<GenerateProjectPipelineCommand, GenerateProjectPipelineResult>
{
    private const string PlainTextContentType = "text/plain";


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

        // Resolve the project-level target (alias "default") to determine base paths within the repo.
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

        // 4. Generate pipeline YAML per config (mono-repo mode: no per-config variables)
        var perConfigResults = new Dictionary<string, PipelineGenerationResult>();

        foreach (var config in configs)
        {
            var generationRequest = configPipelineGenerationService.BuildGenerationRequestForPipeline(
                config,
                project.PipelineVariableGroups,
                project.AgentPoolName,
                bicepBasePath);
            var result = pipelineGenerationEngine.Generate(generationRequest, config.Name, isMonoRepo: true);
            if (result.IsError)
                return result.Errors;

            var infraPipelineResult = result.Value;

            // Generate app pipelines for compute resources in this config
            var appResult = await configPipelineGenerationService.GenerateAppPipelinesAsync(
                    config,
                    generationRequest,
                    isMonoRepo: true,
                    cancellationToken)
                .ConfigureAwait(false);
            if (appResult.IsError)
                return appResult.Errors;

            var generatedAppPipelines = appResult.Value;

            // Merge app pipeline files into the infra result
            if (generatedAppPipelines.Files.Count > 0)
            {
                var mergedFiles = new Dictionary<string, string>(infraPipelineResult.Files);
                foreach (var (path, content) in generatedAppPipelines.Files)
                    mergedFiles[path] = content;

                infraPipelineResult = new PipelineGenerationResult { TemplateFiles = mergedFiles };
            }

            perConfigResults[config.Name] = infraPipelineResult;
        }

        // 5. Collect unique environment definitions across all configs (dedup by ShortName)
        var environments = configs
            .SelectMany(c => c.Environments)
            .GroupBy(e => e.ShortName.ToLowerInvariant())
            .SelectMany(g => g.Take(1))
            .Select(e => new EnvironmentDefinition
            {
                Name = e.Name,
                ShortName = e.ShortName,
                Location = e.Location,
                Prefix = e.Prefix,
                Suffix = e.Suffix,
                AzureResourceManagerConnection = e.AzureResourceManagerConnection,
                SubscriptionId = e.SubscriptionId,
                Tags = e.Tags,
            })
            .ToList();

        // 6. Assemble mono-repo output
        var agentPoolName = project.AgentPoolName;
        var assembled = MonoRepoPipelineAssembler.Assemble(
            perConfigResults,
            environments,
            agentPoolName,
            bicepBasePath,
            pipelineBasePath);

        // 7. Upload to blob storage.
        // Layout (since SplitInfraCode dual-push):
        //   {prefix}/infra/.azuredevops/...   \u2192 infra shared templates
        //   {prefix}/app/.azuredevops/...     \u2192 app shared templates (when any app pipeline exists)
        //   {prefix}/infra/{configName}/...   \u2192 infra per-config files
        //   {prefix}/app/{configName}/...     \u2192 app per-config files (apps/ wrappers)
        var prefix = $"pipeline/project/{command.ProjectId.Value}/{DateTimeOffset.UtcNow:yyyyMMddHHmmss}";

        var infraCommonUris = new Dictionary<string, Uri>(StringComparer.Ordinal);
        var appCommonUris = new Dictionary<string, Uri>(StringComparer.Ordinal);
        foreach (var (path, content) in assembled.CommonFiles)
        {
            var repoRelativePath = $".azuredevops/Common/{path}";
            var uri = await blobService.UploadContentAsync(
                $"{prefix}/infra/{repoRelativePath}", content, PlainTextContentType);
            infraCommonUris[repoRelativePath] = uri;
        }

        // Upload shared application pipeline templates whenever any per-config bucket emitted apps/ wrappers.
        var hasAppPipelines = assembled.ConfigFiles.Values
            .Any(files => files.Keys.Any(p => p.StartsWith("apps/", StringComparison.Ordinal)));

        if (hasAppPipelines)
        {
            foreach (var (path, content) in AppPipelineGenerationEngine.GenerateSharedTemplates())
            {
                var repoRelativePath = ToCommonAzureDevOpsPath(path);
                var uri = await blobService.UploadContentAsync(
                    $"{prefix}/app/{repoRelativePath}", content, PlainTextContentType);
                appCommonUris[repoRelativePath] = uri;
            }
        }

        var infraConfigUris = new Dictionary<string, IReadOnlyDictionary<string, Uri>>(StringComparer.Ordinal);
        var appConfigUris = new Dictionary<string, IReadOnlyDictionary<string, Uri>>(StringComparer.Ordinal);
        var unionConfigUris = new Dictionary<string, IReadOnlyDictionary<string, Uri>>(StringComparer.Ordinal);

        foreach (var (configName, files) in assembled.ConfigFiles)
        {
            var (infraFiles, appFiles) = AppPipelineFileClassifier.Split(files);

            var infraUris = new Dictionary<string, Uri>(StringComparer.Ordinal);
            foreach (var (path, content) in infraFiles)
            {
                var repoRelativePath = $".azuredevops/{configName}/{path}";
                var uri = await blobService.UploadContentAsync(
                    $"{prefix}/infra/{repoRelativePath}", content, PlainTextContentType);
                infraUris[repoRelativePath] = uri;
            }

            var appUris = new Dictionary<string, Uri>(StringComparer.Ordinal);
            foreach (var (path, content) in appFiles)
            {
                var repoRelativePath = $".azuredevops/{configName}/{path}";
                var uri = await blobService.UploadContentAsync(
                    $"{prefix}/app/{repoRelativePath}", content, PlainTextContentType);
                appUris[repoRelativePath] = uri;
            }

            infraConfigUris[configName] = infraUris;
            appConfigUris[configName] = appUris;

            // Union view (legacy CommonFileUris/ConfigFileUris consumers expect a flat per-config map).
            var union = new Dictionary<string, Uri>(infraUris, StringComparer.Ordinal);
            foreach (var (path, uri) in appUris)
                union[path] = uri;
            unionConfigUris[configName] = union;
        }

        var unionCommonUris = new Dictionary<string, Uri>(infraCommonUris, StringComparer.Ordinal);
        foreach (var (path, uri) in appCommonUris)
            unionCommonUris[path] = uri;

        return new GenerateProjectPipelineResult(
            CommonFileUris: unionCommonUris,
            ConfigFileUris: unionConfigUris,
            InfraCommonFileUris: infraCommonUris,
            AppCommonFileUris: appCommonUris,
            InfraConfigFileUris: infraConfigUris,
            AppConfigFileUris: appConfigUris);
    }

    private static string ToCommonAzureDevOpsPath(string path)
    {
        const string azureDevOpsPrefix = ".azuredevops/";

        return path.StartsWith(azureDevOpsPrefix, StringComparison.Ordinal)
            ? $".azuredevops/Common/{path[azureDevOpsPrefix.Length..]}"
            : $".azuredevops/Common/{path}";
    }
}
