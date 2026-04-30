using ErrorOr;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace InfraFlowSculptor.Application.Imports.Common.Creation;

/// <summary>
/// Thin wrapper over <see cref="ResourceCreationCoordinator"/> for the import-apply flow.
/// Maps <see cref="ImportResourceInput"/> to the coordinator's generic input and maps results
/// back to <see cref="ApplyImportPreviewCreatedResourceResult"/>/<see cref="ApplyImportPreviewSkippedResourceResult"/>.
/// </summary>
internal static class ImportResourceCreationDispatcher
{
    internal static Task<ErrorOr<(InfrastructureConfigId ConfigId, ResourceGroupId ResourceGroupId)>> CreateInfrastructureAsync(
        ISender mediator,
        Guid projectId,
        string projectName,
        string location,
        CancellationToken cancellationToken)
    {
        return ResourceCreationCoordinator.CreateInfrastructureAsync(mediator, projectId, projectName, location, cancellationToken);
    }

    internal static async Task<(
        List<ApplyImportPreviewCreatedResourceResult> Created,
        List<ApplyImportPreviewSkippedResourceResult> Skipped)> CreateResourcesAsync(
        ISender mediator,
        ResourceGroupId resourceGroupId,
        IReadOnlyList<ImportResourceInput> resources,
        CancellationToken cancellationToken,
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
            mediator, resourceGroupId, inputs, logger, cancellationToken).ConfigureAwait(false);

        return (
            created.Select(c => new ApplyImportPreviewCreatedResourceResult(c.ResourceType, c.ResourceId, c.Name)).ToList(),
            skipped.Select(s => new ApplyImportPreviewSkippedResourceResult(s.ResourceType, s.Name, s.Reason)).ToList());
    }
}