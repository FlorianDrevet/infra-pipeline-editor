using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.BicepGeneration;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Models;
using InfraFlowSculptor.Domain.Common.Errors;
using MediatR;

namespace InfraFlowSculptor.Application.Projects.Commands.GenerateProjectBicep;

/// <summary>Handles the <see cref="GenerateProjectBicepCommand"/>.</summary>
public sealed class GenerateProjectBicepCommandHandler(
    IProjectAccessService accessService,
    IProjectRepository projectRepository,
    IInfrastructureConfigRepository configRepository,
    IInfrastructureConfigReadRepository configReadRepository,
    BicepGenerationEngine bicepGenerationEngine,
    IBlobService blobService)
    : ICommandHandler<GenerateProjectBicepCommand, GenerateProjectBicepResult>
{
    /// <summary>The subdirectory name where Bicep parameter files are stored.</summary>
    private const string ParametersDirectory = "parameters";



    /// <inheritdoc />
    public async Task<ErrorOr<GenerateProjectBicepResult>> Handle(
        GenerateProjectBicepCommand command,
        CancellationToken cancellationToken)
    {
        // 1. Verify write access
        var authResult = await accessService.VerifyWriteAccessAsync(command.ProjectId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        // 2. Load all configurations for this project
        var configs = await configReadRepository.GetAllByProjectIdWithResourcesAsync(
            command.ProjectId.Value, cancellationToken);

        if (configs.Count == 0)
            return Errors.Project.NoConfigurationsError();

        // 2.bis Ambiguity gate: reject project-level generate-all for heterogeneous multi-repo topologies.
        // Uses domain aggregates to access RepositoryBinding (not exposed by the read model).
        var project = await projectRepository.GetByIdAsync(command.ProjectId, cancellationToken);
        if (project is null)
            return Errors.Project.NotFoundError(command.ProjectId);

        var domainConfigs = await configRepository.GetByProjectIdAsync(command.ProjectId, cancellationToken);
        if (!project.CanGenerateAllFromProjectLevel(domainConfigs))
            return Errors.GitRouting.AmbiguousProjectLevelGeneration;

        // 3. Build per-config generation requests
        var configRequests = new Dictionary<string, GenerationRequest>();
        NamingContext? sharedNamingContext = null;
        var allEnvironments = new List<EnvironmentDefinition>();
        var allEnvironmentNames = new List<string>();

        foreach (var config in configs)
        {
            var generationRequest = GenerationRequestBuilder.Build(config);
            configRequests[PathSanitizer.Sanitize(config.Name)] = generationRequest;

            // Use the first config's naming context and environments as shared (they come from the project)
            if (sharedNamingContext is null)
            {
                sharedNamingContext = generationRequest.NamingContext;
                allEnvironments = generationRequest.Environments.ToList();
                allEnvironmentNames = generationRequest.EnvironmentNames.ToList();
            }
        }

        // 4. Generate mono-repo Bicep files.
        // Shared files remain under Common/ for both AllInOne and SplitInfraCode layouts.
        var monoRepoRequest = new MonoRepoGenerationRequest
        {
            ConfigRequests = configRequests,
            NamingContext = sharedNamingContext!,
            Environments = allEnvironments,
            EnvironmentNames = allEnvironmentNames,
        };

        var result = bicepGenerationEngine.GenerateMonoRepo(monoRepoRequest);

        // 5. Upload to blob storage
        var prefix = $"bicep/project/{command.ProjectId.Value}/{DateTimeOffset.UtcNow:yyyyMMddHHmmss}";
        const string sharedPathSegment = "Common/";

        var commonFileUris = new Dictionary<string, Uri>();
        foreach (var (path, content) in result.CommonFiles)
        {
            var uri = await blobService.UploadContentAsync(
                $"{prefix}/{sharedPathSegment}{path}", content, "text/plain");
            commonFileUris[$"{sharedPathSegment}{path}"] = uri;
        }

        var configFileUris = new Dictionary<string, IReadOnlyDictionary<string, Uri>>();
        foreach (var (configName, files) in result.ConfigFiles)
        {
            var uris = new Dictionary<string, Uri>();
            foreach (var (path, content) in files)
            {
                var uri = await blobService.UploadContentAsync(
                    $"{prefix}/{configName}/{path}", content, "text/plain");
                uris[path] = uri;
            }
            configFileUris[configName] = uris;
        }

        return new GenerateProjectBicepResult(commonFileUris, configFileUris);
    }
}
