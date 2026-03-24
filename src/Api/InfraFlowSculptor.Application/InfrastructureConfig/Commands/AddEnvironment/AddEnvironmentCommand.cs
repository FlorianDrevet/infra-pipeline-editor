using ErrorOr;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.AddEnvironment;

public record AddEnvironmentCommand(
    InfrastructureConfigId InfraConfigId,
    string Name,
    string ShortName,
    string Prefix,
    string Suffix,
    string Location,
    Guid TenantId,
    Guid SubscriptionId,
    int Order,
    bool RequiresApproval,
    IReadOnlyList<(string Name, string Value)> Tags
) : IRequest<ErrorOr<EnvironmentDefinitionResult>>;
