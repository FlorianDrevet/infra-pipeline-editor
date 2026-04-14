using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.UserAssignedIdentities.Commands.DeleteUserAssignedIdentity;

/// <summary>
/// Command to permanently delete a user-assigned managed identity.
/// </summary>
public record DeleteUserAssignedIdentityCommand(
    AzureResourceId Id
) : ICommand<Deleted>;
