using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.RoleAssignments.Commands.RemoveRoleAssignment;

public record RemoveRoleAssignmentCommand(
    AzureResourceId SourceResourceId,
    RoleAssignmentId RoleAssignmentId
) : ICommand<Deleted>;
