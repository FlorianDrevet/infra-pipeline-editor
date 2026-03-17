using ErrorOr;
using InfraFlowSculptor.Application.RoleAssignments.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.RoleAssignments.Commands.AddRoleAssignment;

public record AddRoleAssignmentCommand(
    AzureResourceId SourceResourceId,
    AzureResourceId TargetResourceId,
    string ManagedIdentityType,
    string RoleDefinitionId
) : IRequest<ErrorOr<RoleAssignmentResult>>;
