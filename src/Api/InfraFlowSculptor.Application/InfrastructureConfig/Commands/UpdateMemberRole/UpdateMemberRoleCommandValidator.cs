using FluentValidation;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.UpdateMemberRole;

public class UpdateMemberRoleCommandValidator : AbstractValidator<UpdateMemberRoleCommand>
{
    public UpdateMemberRoleCommandValidator()
    {
        RuleFor(x => x.TargetUserId).NotEmpty();
        RuleFor(x => x.NewRole)
            .NotEmpty()
            .Must(r => Enum.TryParse<Role.RoleEnum>(r, ignoreCase: true, out _))
            .WithMessage("NewRole must be one of: Owner, Contributor, Reader");
    }
}
