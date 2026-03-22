using FluentValidation;

namespace InfraFlowSculptor.Application.Projects.Commands.UpdateProjectMemberRole;

/// <summary>Validates the <see cref="UpdateProjectMemberRoleCommand"/>.</summary>
public sealed class UpdateProjectMemberRoleCommandValidator
    : AbstractValidator<UpdateProjectMemberRoleCommand>
{
    public UpdateProjectMemberRoleCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotNull().WithMessage("Project identifier is required.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User identifier is required.");

        RuleFor(x => x.NewRole)
            .NotEmpty().WithMessage("New role is required.");
    }
}
