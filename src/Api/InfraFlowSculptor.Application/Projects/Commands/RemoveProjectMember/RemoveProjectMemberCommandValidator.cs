using FluentValidation;

namespace InfraFlowSculptor.Application.Projects.Commands.RemoveProjectMember;

/// <summary>Validates the <see cref="RemoveProjectMemberCommand"/> before it is handled.</summary>
public sealed class RemoveProjectMemberCommandValidator : AbstractValidator<RemoveProjectMemberCommand>
{
    /// <summary>Initializes validation rules for removing a project member.</summary>
    public RemoveProjectMemberCommandValidator()
    {
        RuleFor(x => x.ProjectId.Value)
            .NotEmpty().WithMessage("ProjectId is required.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");
    }
}
