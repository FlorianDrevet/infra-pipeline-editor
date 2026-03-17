using ErrorOr;
using InfraFlowSculptor.Application.RoleAssignments.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.RoleAssignments.Queries.ListRoleAssignments;

public record ListRoleAssignmentsQuery(AzureResourceId ResourceId) : IRequest<ErrorOr<List<RoleAssignmentResult>>>;
