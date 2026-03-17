using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.RoleAssignments.Commands.RemoveRoleAssignment;

public record RemoveRoleAssignmentCommand(
    AzureResourceId SourceResourceId,
    RoleAssignmentId RoleAssignmentId
) : IRequest<ErrorOr<Deleted>>;
