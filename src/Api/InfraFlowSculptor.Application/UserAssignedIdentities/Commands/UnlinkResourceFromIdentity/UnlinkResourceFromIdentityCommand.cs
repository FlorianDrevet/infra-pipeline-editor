using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.UserAssignedIdentities.Commands.UnlinkResourceFromIdentity;

/// <summary>
/// Unlinks a source resource from a User-Assigned Identity.
/// Moves all role assignments that reference this UAI from the source resource to the UAI itself,
/// preserving the granted rights on the UAI while removing the consuming resource's association.
/// </summary>
public sealed record UnlinkResourceFromIdentityCommand(
    AzureResourceId IdentityId,
    AzureResourceId SourceResourceId
) : ICommand<Deleted>;
