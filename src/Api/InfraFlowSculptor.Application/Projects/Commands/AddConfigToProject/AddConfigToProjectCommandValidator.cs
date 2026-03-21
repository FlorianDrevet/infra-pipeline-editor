using FluentValidation;

namespace InfraFlowSculptor.Application.Projects.Commands.AddConfigToProject;

/// <summary>Validates the <see cref="AddConfigToProjectCommand"/> before it is handled.</summary>
public sealed class AddConfigToProjectCommandValidator : AbstractValidator<AddConfigToProjectCommand>
{
    public AddConfigToProjectCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("ProjectId is required.");

        RuleFor(x => x.ConfigId)
            .NotEmpty().WithMessage("ConfigId is required.");
    }
}
