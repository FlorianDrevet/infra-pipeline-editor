using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Application.UserAssignedIdentities.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.UserAssignedIdentities.Queries.GetUserAssignedIdentity;

/// <summary>
/// Query to retrieve a single user-assigned managed identity by its identifier.
/// </summary>
public record GetUserAssignedIdentityQuery(
    AzureResourceId Id
) : IQuery<UserAssignedIdentityResult>;
