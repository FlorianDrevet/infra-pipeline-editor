using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.Application.InfrastructureConfig.ReadModels;
using InfraFlowSculptor.Application.Projects.Common.Storage;
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
    IInfrastructureConfigReadRepository configReadRepository,
    BicepGenerationEngine bicepGenerationEngine,
    IMonoRepoBlobUploadOrchestrator blobUploadOrchestrator)
    : ICommandHandler<GenerateProjectBicepCommand, GenerateProjectBicepResult>
{
    private const string ContainerRegistryIdPropertyName = "containerRegistryId";

    /// <inheritdoc />
    public async Task<ErrorOr<GenerateProjectBicepResult>> Handle(
        GenerateProjectBicepCommand command,
        CancellationToken cancellationToken)
    {
        // 1. Verify write access
        var authResult = await accessService.VerifyWriteAccessAsync(command.ProjectId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        var project = authResult.Value;

        // 2. Load all configurations for this project
        var configs = await configReadRepository.GetAllByProjectIdWithResourcesAsync(
            command.ProjectId.Value, cancellationToken);

        if (configs.Count == 0)
            return Errors.Project.NoConfigurationsError();

        // Reject project-level generate-all for heterogeneous multi-repo topologies.
        if (!project.CanGenerateAllFromProjectLevel())
            return Errors.GitRouting.AmbiguousProjectLevelGeneration;

        // 3. Build per-config generation requests
        var configRequests = new Dictionary<string, GenerationRequest>();
        var configsBySanitizedName = new Dictionary<string, InfrastructureConfigReadModel>(StringComparer.OrdinalIgnoreCase);
        NamingContext? sharedNamingContext = null;
        var allEnvironments = new List<EnvironmentDefinition>();
        var allEnvironmentNames = new List<string>();

        foreach (var config in configs)
        {
            var generationRequest = GenerationRequestBuilder.Build(config);
            var sanitizedConfigName = PathSanitizer.Sanitize(config.Name);
            configRequests[sanitizedConfigName] = generationRequest;
            configsBySanitizedName[sanitizedConfigName] = config;

            // Use the first config's naming context and environments as shared (they come from the project)
            if (sharedNamingContext is null)
            {
                sharedNamingContext = generationRequest.NamingContext;
                allEnvironments = generationRequest.Environments.ToList();
                allEnvironmentNames = generationRequest.EnvironmentNames.ToList();
            }
        }

        InferCrossConfigContainerRegistryReferences(configsBySanitizedName, configRequests);

        // 4. Generate mono-repo Bicep files.
        // Shared files remain under Common/ for both AllInOne and SplitInfraCode layouts.
        var monoRepoRequest = new MonoRepoGenerationRequest
        {
            ConfigRequests = configRequests,
            NamingContext = sharedNamingContext!,
            Environments = allEnvironments,
            EnvironmentNames = allEnvironmentNames,
        };

        var result = bicepGenerationEngine.GenerateMonoRepo(monoRepoRequest, cancellationToken);
        if (result.IsError)
            return result.Errors;

        var monoRepoResult = result.Value;

        // 5. Upload to blob storage.
        var prefix = $"bicep/project/{command.ProjectId.Value}/{DateTimeOffset.UtcNow:yyyyMMddHHmmss}";
        var uploadResult = await blobUploadOrchestrator.UploadBicepAsync(
                prefix,
                monoRepoResult,
                cancellationToken)
            .ConfigureAwait(false);

        return new GenerateProjectBicepResult(uploadResult.CommonFileUris, uploadResult.ConfigFileUris);
    }

    private static void InferCrossConfigContainerRegistryReferences(
        IReadOnlyDictionary<string, InfrastructureConfigReadModel> configsBySanitizedName,
        IDictionary<string, GenerationRequest> configRequests)
    {
        var crossConfigResources = BuildCrossConfigResourceLookup(configsBySanitizedName);

        foreach (var (configName, generationRequest) in configRequests)
        {
            if (!configsBySanitizedName.TryGetValue(configName, out var config))
                continue;

            var existingReferenceKeys = generationRequest.ExistingResourceReferences
                .Select(reference => BuildExistingResourceKey(reference.ResourceName, reference.ResourceType, reference.ResourceGroupName))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var inferredReferences = new List<ExistingResourceReference>();
            foreach (var resourceGroup in config.ResourceGroups)
            {
                foreach (var resource in resourceGroup.Resources)
                {
                    if (!TryResolveCrossConfigContainerRegistryReference(configName, resource, crossConfigResources, out var crossConfigResource))
                        continue;

                    var referenceKey = BuildExistingResourceKey(
                        crossConfigResource.ResourceName,
                        crossConfigResource.ResourceType,
                        crossConfigResource.ResourceGroupName);

                    if (!existingReferenceKeys.Add(referenceKey))
                        continue;

                    inferredReferences.Add(new ExistingResourceReference
                    {
                        ResourceName = crossConfigResource.ResourceName,
                        TargetResourceId = crossConfigResource.ResourceId,
                        ResourceTypeName = crossConfigResource.ResourceTypeName,
                        ResourceType = crossConfigResource.ResourceType,
                        ResourceGroupName = crossConfigResource.ResourceGroupName,
                        ResourceAbbreviation = crossConfigResource.ResourceAbbreviation,
                        SourceConfigName = crossConfigResource.ConfigName,
                    });
                }
            }

            if (inferredReferences.Count == 0)
                continue;

            generationRequest.ExistingResourceReferences = generationRequest.ExistingResourceReferences
                .Concat(inferredReferences)
                .ToList();
        }
    }

    private static Dictionary<Guid, CrossConfigResourceDescriptor> BuildCrossConfigResourceLookup(
        IReadOnlyDictionary<string, InfrastructureConfigReadModel> configsBySanitizedName)
    {
        var resourceLookup = new Dictionary<Guid, CrossConfigResourceDescriptor>();

        foreach (var (configKey, config) in configsBySanitizedName)
        {
            var mergedAbbreviations = GenerationRequestBuilder.MergeAbbreviations(config.NamingContext.ResourceAbbreviations);

            foreach (var resourceGroup in config.ResourceGroups)
            {
                foreach (var resource in resourceGroup.Resources)
                {
                    var resourceTypeName = GenerationRequestBuilder.GetResourceTypeName(resource.ResourceType);
                    resourceLookup[resource.Id] = new CrossConfigResourceDescriptor(
                        ResourceId: resource.Id,
                        ConfigKey: configKey,
                        ConfigName: config.Name,
                        ResourceName: resource.Name,
                        ResourceTypeName: resourceTypeName,
                        ResourceType: resource.ResourceType,
                        ResourceGroupName: resourceGroup.Name,
                        ResourceAbbreviation: GenerationRequestBuilder.GetResourceAbbreviation(resource.ResourceType, mergedAbbreviations));
                }
            }
        }

        return resourceLookup;
    }

    private static bool TryResolveCrossConfigContainerRegistryReference(
        string currentConfigName,
        AzureResourceReadModel resource,
        IReadOnlyDictionary<Guid, CrossConfigResourceDescriptor> crossConfigResources,
        out CrossConfigResourceDescriptor crossConfigResource)
    {
        crossConfigResource = default!;

        if (!resource.Properties.TryGetValue(ContainerRegistryIdPropertyName, out var containerRegistryIdValue)
            || !Guid.TryParse(containerRegistryIdValue, out var containerRegistryId))
        {
            return false;
        }

        if (!crossConfigResources.TryGetValue(containerRegistryId, out var resolvedResource)
            || string.Equals(resolvedResource.ConfigKey, currentConfigName, StringComparison.OrdinalIgnoreCase)
            || !string.Equals(resolvedResource.ResourceType, AzureResourceTypes.ArmTypes.ContainerRegistryType, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        crossConfigResource = resolvedResource;
        return true;
    }

    private static string BuildExistingResourceKey(string resourceName, string resourceType, string resourceGroupName)
    {
        return string.Create(
            resourceName.Length + resourceType.Length + resourceGroupName.Length + 2,
            (resourceName, resourceType, resourceGroupName),
            static (buffer, state) =>
            {
                var position = 0;
                state.resourceName.AsSpan().CopyTo(buffer[position..]);
                position += state.resourceName.Length;
                buffer[position++] = '|';
                state.resourceType.AsSpan().CopyTo(buffer[position..]);
                position += state.resourceType.Length;
                buffer[position++] = '|';
                state.resourceGroupName.AsSpan().CopyTo(buffer[position..]);
            });
    }

    private sealed record CrossConfigResourceDescriptor(
        Guid ResourceId,
        string ConfigKey,
        string ConfigName,
        string ResourceName,
        string ResourceTypeName,
        string ResourceType,
        string ResourceGroupName,
        string ResourceAbbreviation);
}
