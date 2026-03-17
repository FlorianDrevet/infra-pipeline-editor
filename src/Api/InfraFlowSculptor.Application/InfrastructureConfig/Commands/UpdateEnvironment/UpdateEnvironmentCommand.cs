using ErrorOr;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.UpdateEnvironment;

public record UpdateEnvironmentCommand(
    InfrastructureConfigId InfraConfigId,
    EnvironmentDefinitionId EnvironmentId,
    string Name,
    string Prefix,
    string Suffix,
    string Location,
    Guid TenantId,
    Guid SubscriptionId,
    int Order,
    bool RequiresApproval,
    IReadOnlyList<(string Name, string Value)> Tags
) : IRequest<ErrorOr<EnvironmentDefinitionResult>>;
