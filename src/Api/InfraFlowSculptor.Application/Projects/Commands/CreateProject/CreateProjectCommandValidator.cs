using FluentValidation;

namespace InfraFlowSculptor.Application.Projects.Commands.CreateProject;

/// <summary>Validates the <see cref="CreateProjectCommand"/>.</summary>
public sealed class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Project name is required.")
            .MaximumLength(100).WithMessage("Project name must not exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Project description must not exceed 500 characters.");
    }
}
