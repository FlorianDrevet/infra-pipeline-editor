using FluentValidation;

namespace InfraFlowSculptor.Application.Projects.Commands.AddProjectMember;

/// <summary>Validates the <see cref="AddProjectMemberCommand"/> before it is handled.</summary>
public sealed class AddProjectMemberCommandValidator : AbstractValidator<AddProjectMemberCommand>
{
    private static readonly string[] ValidRoles = ["Owner", "Contributor", "Reader"];

    public AddProjectMemberCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("ProjectId is required.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required.")
            .Must(r => ValidRoles.Contains(r)).WithMessage("Role must be Owner, Contributor, or Reader.");
    }
}
