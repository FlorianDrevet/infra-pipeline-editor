using ErrorOr;
using InfraFlowSculptor.Application.Imports.Common.Creation;
using InfraFlowSculptor.Mcp.Tools.Models;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace InfraFlowSculptor.Mcp.Tools;

/// <summary>
/// Thin wrapper over <see cref="ResourceCreationCoordinator"/> for the MCP project setup flow.
/// Maps <see cref="ResourceInput"/> to the coordinator's generic input and maps results
/// back to <see cref="CreatedResourceInfo"/>/<see cref="SkippedResourceInfo"/>.
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
        var result = await ResourceCreationCoordinator.CreateInfrastructureAsync(
            mediator, projectId, projectName, location, ct).ConfigureAwait(false);

        if (result.IsError)
            return result.Errors;

        return (result.Value.ConfigId, result.Value.ResourceGroupId);
    }

    /// <summary>
    /// Creates resources from a list of inputs, resolving dependencies.
    /// </summary>
    public static async Task<(List<CreatedResourceInfo> Created, List<SkippedResourceInfo> Skipped)> CreateResourcesAsync(
        ISender mediator,
        ResourceGroupId resourceGroupId,
        IReadOnlyList<ResourceInput> resources,
        CancellationToken ct = default,
        ILogger? logger = null)
    {
        var inputs = resources
            .Select(r => new ResourceCreationInput
            {
                ResourceType = r.ResourceType,
                Name = r.Name,
                Location = r.Location,
                DependencyResourceNames = r.DependencyResourceNames,
                ExtractedProperties = r.ExtractedProperties,
            })
            .ToList();

        var (created, skipped) = await ResourceCreationCoordinator.CreateResourcesAsync(
            mediator, resourceGroupId, inputs, logger, ct).ConfigureAwait(false);

        return (
            created.Select(c => new CreatedResourceInfo
            {
                ResourceType = c.ResourceType,
                ResourceId = c.ResourceId,
                Name = c.Name,
            }).ToList(),
            skipped.Select(s => new SkippedResourceInfo
            {
                ResourceType = s.ResourceType,
                Name = s.Name,
                Reason = s.Reason,
            }).ToList());
    }
}
