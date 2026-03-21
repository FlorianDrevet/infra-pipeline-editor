using FluentValidation;

namespace InfraFlowSculptor.Application.Projects.Commands.UpdateProjectMemberRole;

/// <summary>Validates the <see cref="UpdateProjectMemberRoleCommand"/> before it is handled.</summary>
public sealed class UpdateProjectMemberRoleCommandValidator : AbstractValidator<UpdateProjectMemberRoleCommand>
{
    private static readonly string[] ValidRoles = ["Owner", "Contributor", "Reader"];

    public UpdateProjectMemberRoleCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("ProjectId is required.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");

        RuleFor(x => x.NewRole)
            .NotEmpty().WithMessage("NewRole is required.")
            .Must(r => ValidRoles.Contains(r)).WithMessage("NewRole must be Owner, Contributor, or Reader.");
    }
}
