using FluentValidation;

namespace InfraFlowSculptor.Application.Projects.Commands.CreateProject;

/// <summary>Validates the <see cref="CreateProjectCommand"/> before it is handled.</summary>
public sealed class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Project name is required.")
            .MaximumLength(80).WithMessage("Project name must not exceed 80 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Project description must not exceed 500 characters.")
            .When(x => x.Description is not null);
    }
}
