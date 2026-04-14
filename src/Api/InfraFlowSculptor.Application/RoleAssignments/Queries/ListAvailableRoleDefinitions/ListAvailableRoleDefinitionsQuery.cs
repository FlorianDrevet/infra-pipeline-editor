using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Application.RoleAssignments.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.RoleAssignments.Queries.ListAvailableRoleDefinitions;

public record ListAvailableRoleDefinitionsQuery(AzureResourceId ResourceId)
    : IQuery<List<AzureRoleDefinitionResult>>;
