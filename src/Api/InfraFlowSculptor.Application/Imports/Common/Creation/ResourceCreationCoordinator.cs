using ErrorOr;
using InfraFlowSculptor.Application.Imports.Common.Properties;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.CreateInfraConfig;
using InfraFlowSculptor.Application.ResourceGroup.Commands.CreateResourceGroup;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace InfraFlowSculptor.Application.Imports.Common.Creation;

/// <summary>
/// Single implementation of the infrastructure + resource creation orchestration loop.
/// Both <c>ImportResourceCreationDispatcher</c> (API import-apply) and <c>ProjectSetupOrchestrator</c> (MCP)
/// delegate to this coordinator and map results into their own boundary types.
/// </summary>
public static class ResourceCreationCoordinator
{
    /// <summary>
    /// Creates the infrastructure config and default resource group for a project.
    /// </summary>
    public static async Task<ErrorOr<(InfrastructureConfigId ConfigId, ResourceGroupId ResourceGroupId)>> CreateInfrastructureAsync(
        ISender mediator,
        Guid projectId,
        string projectName,
        string location,
        CancellationToken cancellationToken)
    {
        var configCommand = new CreateInfrastructureConfigCommand(
            Name: $"{projectName}-config",
            ProjectId: projectId);
        var configResult = await mediator.Send(configCommand, cancellationToken).ConfigureAwait(false);

        if (configResult.IsError)
            return configResult.Errors;

        var resourceGroupCommand = new CreateResourceGroupCommand(
            InfraConfigId: configResult.Value.Id,
            Name: new Name($"{projectName}-rg"),
            Location: ParseLocation(location));
        var resourceGroupResult = await mediator.Send(resourceGroupCommand, cancellationToken).ConfigureAwait(false);

        if (resourceGroupResult.IsError)
            return resourceGroupResult.Errors;

        return (configResult.Value.Id, resourceGroupResult.Value.Id);
    }

    /// <summary>
    /// Creates resources from a list of inputs, resolving dependencies via topological ordering.
    /// Logs unexpected failures when a logger is provided, instead of swallowing them silently.
    /// </summary>
    public static async Task<(IReadOnlyList<CreatedResourceResult> Created, IReadOnlyList<SkippedResourceResult> Skipped)> CreateResourcesAsync(
        ISender mediator,
        ResourceGroupId resourceGroupId,
        IReadOnlyList<ResourceCreationInput> resources,
        ILogger? logger = null,
        CancellationToken cancellationToken = default)
    {
        var created = new List<CreatedResourceResult>();
        var skipped = new List<SkippedResourceResult>();
        var createdIdsByType = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
        var createdIdsByName = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);

        var typeCount = resources
            .GroupBy(r => r.ResourceType, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);

        var ordered = ResourceCommandFactory.OrderByDependency(
            resources.Select(r => (r.ResourceType, r.Name)));

        foreach (var (resourceType, name) in ordered)
        {
            var input = resources.FirstOrDefault(r =>
                string.Equals(r.ResourceType, resourceType, StringComparison.OrdinalIgnoreCase)
                && string.Equals(r.Name, name, StringComparison.Ordinal));

            var requiredDep = ResourceCommandFactory.GetRequiredDependencyType(resourceType);
            if (requiredDep is not null
                && (input?.DependencyResourceNames is null or { Count: 0 })
                && typeCount.TryGetValue(requiredDep, out var depCount) && depCount > 1)
            {
                skipped.Add(new SkippedResourceResult(
                    resourceType,
                    name,
                    $"Ambiguous dependency: {depCount} resources of type '{requiredDep}' exist but no explicit dependency name was provided."));
                continue;
            }

            var location = ParseLocation(input?.Location);

            var context = new ResourceCreationContext(
                createdIdsByType,
                ExtractedPropertiesResolver.FromDictionary(resourceType, input?.ExtractedProperties),
                createdIdsByName,
                input?.DependencyResourceNames);

            var createTask = ResourceCommandFactory.CreateResourceAsync(
                mediator,
                resourceType,
                resourceGroupId,
                new Name(name),
                location,
                context,
                cancellationToken);

            if (createTask is null)
            {
                var missingDependency = ResourceCommandFactory.GetMissingDependency(
                    resourceType,
                    createdIdsByType,
                    createdIdsByName,
                    input?.DependencyResourceNames);
                skipped.Add(new SkippedResourceResult(
                    resourceType,
                    name,
                    missingDependency is not null
                        ? $"Dependency '{missingDependency}' was not created or not available."
                        : $"Resource type '{resourceType}' is not supported for auto-creation."));
                continue;
            }

            try
            {
                var result = await createTask.ConfigureAwait(false);

                if (result.IsError)
                {
                    skipped.Add(new SkippedResourceResult(
                        resourceType,
                        name,
                        ResourceCommandFactory.FormatErrors(result.Errors)));
                    continue;
                }

                createdIdsByType[resourceType] = result.Value;
                createdIdsByName[name] = result.Value;
                created.Add(new CreatedResourceResult(
                    resourceType,
                    result.Value.ToString(),
                    name));
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Unexpected error creating resource {ResourceType} '{Name}'", resourceType, name);
                skipped.Add(new SkippedResourceResult(
                    resourceType,
                    name,
                    $"An unexpected error occurred during resource creation ({ex.GetType().Name})."));
            }
        }

        return (created, skipped);
    }

    private static Location ParseLocation(string? location)
    {
        return Enum.TryParse<Location.LocationEnum>(location, true, out var locationEnum)
            ? new Location(locationEnum)
            : new Location(Location.LocationEnum.WestEurope);
    }
}
