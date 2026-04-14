using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.RoleAssignments.Commands.UnassignIdentityFromResource;

/// <summary>Command to remove the assigned User-Assigned Identity from a resource.</summary>
/// <param name="ResourceId">The resource to unassign the identity from.</param>
public record UnassignIdentityFromResourceCommand(
    AzureResourceId ResourceId) : ICommand<Success>;
