using ErrorOr;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.AddMember;

public record AddMemberCommand(
    InfrastructureConfigId InfraConfigId,
    Guid TargetUserId,
    string Role
) : IRequest<ErrorOr<GetInfrastructureConfigResult>>;
