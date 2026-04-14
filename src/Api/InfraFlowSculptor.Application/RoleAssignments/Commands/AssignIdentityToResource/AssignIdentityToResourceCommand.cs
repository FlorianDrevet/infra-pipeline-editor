using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.RoleAssignments.Commands.AssignIdentityToResource;

/// <summary>Command to assign a User-Assigned Identity to a resource.</summary>
/// <param name="ResourceId">The resource to assign the identity to.</param>
/// <param name="UserAssignedIdentityId">The UAI to assign.</param>
public record AssignIdentityToResourceCommand(
    AzureResourceId ResourceId,
    AzureResourceId UserAssignedIdentityId) : ICommand<Success>;
