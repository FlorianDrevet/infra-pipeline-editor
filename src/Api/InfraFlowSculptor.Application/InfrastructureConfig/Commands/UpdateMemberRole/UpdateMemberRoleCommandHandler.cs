using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.UpdateMemberRole;

public class UpdateMemberRoleCommandHandler(
    IInfrastructureConfigRepository repository,
    ICurrentUser currentUser,
    IMapper mapper)
    : IRequestHandler<UpdateMemberRoleCommand, ErrorOr<GetInfrastructureConfigResult>>
{
    public async Task<ErrorOr<GetInfrastructureConfigResult>> Handle(
        UpdateMemberRoleCommand command, CancellationToken cancellationToken)
    {
        var authResult = await MemberCommandHelper.AuthorizeOwnerAndFindTargetAsync(
            repository, currentUser, command.InfraConfigId, command.TargetUserId, cancellationToken);

        if (authResult.IsError)
            return authResult.Errors;

        var (infraConfig, targetUserId) = authResult.Value;

        var newRole = new Role(Enum.Parse<Role.RoleEnum>(command.NewRole, ignoreCase: true));
        infraConfig.ChangeRole(targetUserId, newRole);

        var saved = await repository.UpdateAsync(infraConfig);
        return mapper.Map<GetInfrastructureConfigResult>(saved);
    }
}
