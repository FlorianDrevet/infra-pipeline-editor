using ErrorOr;
using InfraFlowSculptor.Application.UserAssignedIdentities.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.UserAssignedIdentities.Commands.UpdateUserAssignedIdentity;

/// <summary>
/// Command to update an existing user-assigned managed identity.
/// </summary>
public record UpdateUserAssignedIdentityCommand(
    AzureResourceId Id,
    Name Name,
    Location Location
) : IRequest<ErrorOr<UserAssignedIdentityResult>>;
