using ErrorOr;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.UpdateMemberRole;

public record UpdateMemberRoleCommand(
    InfrastructureConfigId InfraConfigId,
    Guid TargetUserId,
    string NewRole
) : IRequest<ErrorOr<GetInfrastructureConfigResult>>;
