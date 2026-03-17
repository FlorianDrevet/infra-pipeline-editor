using ErrorOr;
using InfraFlowSculptor.Application.RoleAssignments.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.RoleAssignments.Queries.ListAvailableRoleDefinitions;

public record ListAvailableRoleDefinitionsQuery(AzureResourceId ResourceId)
    : IRequest<ErrorOr<List<AzureRoleDefinitionResult>>>;
