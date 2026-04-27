using FluentValidation;

namespace InfraFlowSculptor.Application.Projects.Commands.DeleteProject;

/// <summary>Validates the <see cref="DeleteProjectCommand"/>.</summary>
public sealed class DeleteProjectCommandValidator : AbstractValidator<DeleteProjectCommand>
{
    public DeleteProjectCommandValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty().WithMessage("ProjectId is required.");
    }
}
