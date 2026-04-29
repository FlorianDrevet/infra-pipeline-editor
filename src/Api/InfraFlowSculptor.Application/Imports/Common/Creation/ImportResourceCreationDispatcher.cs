using ErrorOr;
using InfraFlowSculptor.Application.Imports.Common.Properties;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.CreateInfraConfig;
using InfraFlowSculptor.Application.ResourceGroup.Commands.CreateResourceGroup;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.Imports.Common.Creation;

/// <summary>
/// Orchestrates infrastructure and resource creation for the import apply flow.
/// Delegates command building, dependency ordering, and dispatch to <see cref="ResourceCommandFactory"/>.
/// </summary>
internal static class ImportResourceCreationDispatcher
{
    internal static async Task<ErrorOr<(InfrastructureConfigId ConfigId, ResourceGroupId ResourceGroupId)>> CreateInfrastructureAsync(
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

    internal static async Task<(
        List<ApplyImportPreviewCreatedResourceResult> Created,
        List<ApplyImportPreviewSkippedResourceResult> Skipped)> CreateResourcesAsync(
        ISender mediator,
        ResourceGroupId resourceGroupId,
        IReadOnlyList<ImportResourceInput> resources,
        CancellationToken cancellationToken)
    {
        var created = new List<ApplyImportPreviewCreatedResourceResult>();
        var skipped = new List<ApplyImportPreviewSkippedResourceResult>();
        var createdIdsByType = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
        var createdIdsByName = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);

        var ordered = ResourceCommandFactory.OrderByDependency(
            resources.Select(resource => (resource.ResourceType, resource.Name)));

        foreach (var (resourceType, name) in ordered)
        {
            var input = resources.FirstOrDefault(resource =>
                string.Equals(resource.ResourceType, resourceType, StringComparison.OrdinalIgnoreCase)
                && string.Equals(resource.Name, name, StringComparison.Ordinal));

            var location = ParseLocation(input?.Location);

            var context = new ResourceCreationContext(
                createdIdsByType,
                ExtractedPropertiesResolver.FromDictionary(resourceType, input?.ExtractedProperties),
                createdIdsByName,
                input?.DependencyResourceNames);

            var command = ResourceCommandFactory.BuildCommand(
                resourceType,
                resourceGroupId,
                new Name(name),
                location,
                context);

            if (command is null)
            {
                var missingDependency = ResourceCommandFactory.GetMissingDependency(
                    resourceType,
                    createdIdsByType,
                    createdIdsByName,
                    input?.DependencyResourceNames);
                skipped.Add(new ApplyImportPreviewSkippedResourceResult(
                    resourceType,
                    name,
                    missingDependency is not null
                        ? $"Dependency '{missingDependency}' was not created or not available."
                        : $"Resource type '{resourceType}' is not supported for auto-creation."));
                continue;
            }

            try
            {
                var result = await ResourceCommandFactory.SendCommandAsync(mediator, command, cancellationToken).ConfigureAwait(false);
                var resourceId = ResourceCommandFactory.ExtractResourceId(result);

                if (resourceId is null)
                {
                    skipped.Add(new ApplyImportPreviewSkippedResourceResult(
                        resourceType,
                        name,
                        ResourceCommandFactory.ExtractErrors(result) ?? "Creation failed."));
                    continue;
                }

                createdIdsByType[resourceType] = resourceId.Value;
                createdIdsByName[name] = resourceId.Value;
                created.Add(new ApplyImportPreviewCreatedResourceResult(
                    resourceType,
                    resourceId.Value.ToString(),
                    name));
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                skipped.Add(new ApplyImportPreviewSkippedResourceResult(
                    resourceType,
                    name,
                    $"Unexpected error: {ex.Message}"));
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