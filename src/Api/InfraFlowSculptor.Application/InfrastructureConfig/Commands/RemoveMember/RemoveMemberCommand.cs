using ErrorOr;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.RemoveMember;

public record RemoveMemberCommand(
    InfrastructureConfigId InfraConfigId,
    Guid TargetUserId
) : IRequest<ErrorOr<Deleted>>;
