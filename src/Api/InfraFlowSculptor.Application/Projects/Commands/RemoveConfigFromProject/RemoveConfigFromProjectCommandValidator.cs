using FluentValidation;

namespace InfraFlowSculptor.Application.Projects.Commands.RemoveConfigFromProject;

/// <summary>Validates the <see cref="RemoveConfigFromProjectCommand"/> before it is handled.</summary>
public sealed class RemoveConfigFromProjectCommandValidator : AbstractValidator<RemoveConfigFromProjectCommand>
{
    public RemoveConfigFromProjectCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("ProjectId is required.");

        RuleFor(x => x.ConfigId)
            .NotEmpty().WithMessage("ConfigId is required.");
    }
}
