using ErrorOr;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.CreateInfraConfig;
using InfraFlowSculptor.Application.ResourceGroup.Commands.CreateResourceGroup;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using MediatR;
using Location = InfraFlowSculptor.Domain.Common.ValueObjects.Location;

namespace InfraFlowSculptor.Mcp.Tools;

/// <summary>
/// Orchestrates the creation of a project with its infrastructure config, resource group, and resources.
/// Shared between <see cref="ProjectCreationTools"/> and <see cref="IacImportTools"/>.
/// </summary>
public static class ProjectSetupOrchestrator
{
    /// <summary>
    /// Creates the infrastructure config and default resource group for a project.
    /// </summary>
    public static async Task<ErrorOr<(InfrastructureConfigId ConfigId, ResourceGroupId RgId)>> CreateInfrastructureAsync(
        ISender mediator,
        Guid projectId,
        string projectName,
        string location,
        CancellationToken ct = default)
    {
        var configCommand = new CreateInfrastructureConfigCommand(
            Name: $"{projectName}-config",
            ProjectId: projectId);
        var configResult = await mediator.Send(configCommand, ct);

        if (configResult.IsError)
            return configResult.Errors;

        var rgCommand = new CreateResourceGroupCommand(
            InfraConfigId: configResult.Value.Id,
            Name: new Name($"{projectName}-rg"),
            Location: ParseLocation(location));
        var rgResult = await mediator.Send(rgCommand, ct);

        if (rgResult.IsError)
            return rgResult.Errors;

        return (configResult.Value.Id, rgResult.Value.Id);
    }

    /// <summary>
    /// Creates resources from a list of inputs, resolving dependencies.
    /// </summary>
    public static async Task<(List<CreatedResourceInfo> Created, List<SkippedResourceInfo> Skipped)> CreateResourcesAsync(
        ISender mediator,
        ResourceGroupId resourceGroupId,
        IReadOnlyList<ResourceInput> resources,
        CancellationToken ct = default)
    {
        var created = new List<CreatedResourceInfo>();
        var skipped = new List<SkippedResourceInfo>();
        var createdIdsByType = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
        var createdIdsByName = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);

        var ordered = ResourceCommandFactory.OrderByDependency(
            resources.Select(r => (r.ResourceType, r.Name)));

        foreach (var (resourceType, name) in ordered)
        {
            var input = resources.FirstOrDefault(r =>
                string.Equals(r.ResourceType, resourceType, StringComparison.OrdinalIgnoreCase)
                && r.Name == name);

            var location = ParseLocation(input?.Location);
            var context = new ResourceCreationContext(
                createdIdsByType, input?.ExtractedProperties, createdIdsByName, input?.DependencyResourceNames);
            var command = ResourceCommandFactory.BuildCommand(
                resourceType, resourceGroupId, new Name(name), location,
                context);

            if (command is null)
            {
                var missing = ResourceCommandFactory.GetMissingDependency(
                    resourceType,
                    createdIdsByType,
                    createdIdsByName,
                    input?.DependencyResourceNames);
                skipped.Add(new SkippedResourceInfo
                {
                    ResourceType = resourceType,
                    Name = name,
                    Reason = missing is not null
                        ? $"Dependency '{missing}' was not created or not available."
                        : $"Resource type '{resourceType}' is not supported for auto-creation.",
                });
                continue;
            }

            try
            {
                var result = await mediator.Send(command, ct);
                var resourceId = ExtractResourceId(result);

                if (resourceId is null)
                {
                    var errors = ExtractErrors(result);
                    skipped.Add(new SkippedResourceInfo
                    {
                        ResourceType = resourceType,
                        Name = name,
                        Reason = errors ?? "Creation failed.",
                    });
                    continue;
                }

                createdIdsByType[resourceType] = resourceId.Value;
                createdIdsByName[name] = resourceId.Value;
                created.Add(new CreatedResourceInfo
                {
                    ResourceType = resourceType,
                    ResourceId = resourceId.Value.ToString(),
                    Name = name,
                });
            }
            catch (Exception ex)
            {
                skipped.Add(new SkippedResourceInfo
                {
                    ResourceType = resourceType,
                    Name = name,
                    Reason = $"Unexpected error: {ex.Message}",
                });
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

    /// <summary>Extracts the AzureResourceId.Value from an <c>ErrorOr&lt;T&gt;</c> result using reflection.</summary>
    private static Guid? ExtractResourceId(object? result)
    {
        if (result is null)
            return null;

        var resultType = result.GetType();

        var isErrorProp = resultType.GetProperty("IsError");
        if (isErrorProp is not null && isErrorProp.GetValue(result) is true)
            return null;

        var valueProp = resultType.GetProperty("Value");
        var value = valueProp?.GetValue(result);
        if (value is null)
            return null;

        var idProp = value.GetType().GetProperty("Id");
        var id = idProp?.GetValue(value);
        if (id is null)
            return null;

        var guidProp = id.GetType().GetProperty("Value");
        return guidProp?.GetValue(id) as Guid?;
    }

    /// <summary>Extracts error descriptions from an <c>ErrorOr&lt;T&gt;</c> result using reflection.</summary>
    private static string? ExtractErrors(object? result)
    {
        if (result is null)
            return null;

        var resultType = result.GetType();
        var errorsProp = resultType.GetProperty("Errors");
        if (errorsProp?.GetValue(result) is not IEnumerable<Error> errors)
            return null;

        return string.Join("; ", errors.Select(e => e.Description));
    }
}
