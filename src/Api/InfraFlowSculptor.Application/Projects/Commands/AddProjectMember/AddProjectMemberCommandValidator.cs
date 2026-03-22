using FluentValidation;

namespace InfraFlowSculptor.Application.Projects.Commands.AddProjectMember;

/// <summary>Validates the <see cref="AddProjectMemberCommand"/>.</summary>
public sealed class AddProjectMemberCommandValidator : AbstractValidator<AddProjectMemberCommand>
{
    public AddProjectMemberCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotNull().WithMessage("Project identifier is required.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User identifier is required.");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required.");
    }
}
