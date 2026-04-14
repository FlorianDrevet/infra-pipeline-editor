using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Application.UserAssignedIdentities.Common;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.UserAssignedIdentities.Commands.CreateUserAssignedIdentity;

/// <summary>
/// Command to create a new user-assigned managed identity inside a resource group.
/// </summary>
public record CreateUserAssignedIdentityCommand(
    ResourceGroupId ResourceGroupId,
    Name Name,
    Location Location
) : ICommand<UserAssignedIdentityResult>;
