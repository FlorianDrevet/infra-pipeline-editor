using FluentValidation;

namespace InfraFlowSculptor.Application.Projects.Commands.SetRepositoryMode;

/// <summary>Validates the <see cref="SetRepositoryModeCommand"/> before it is handled.</summary>
public sealed class SetRepositoryModeCommandValidator
    : AbstractValidator<SetRepositoryModeCommand>
{
    public SetRepositoryModeCommandValidator()
    {
        RuleFor(x => x.ProjectId.Value)
            .NotEmpty().WithMessage("ProjectId is required.");

        RuleFor(x => x.RepositoryMode)
            .NotEmpty().WithMessage("RepositoryMode is required.");
    }
}
