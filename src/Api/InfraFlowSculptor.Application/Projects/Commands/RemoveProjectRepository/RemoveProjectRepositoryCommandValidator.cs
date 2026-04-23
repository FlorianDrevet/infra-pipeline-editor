using FluentValidation;

namespace InfraFlowSculptor.Application.Projects.Commands.RemoveProjectRepository;

/// <summary>Validates the <see cref="RemoveProjectRepositoryCommand"/> before it is handled.</summary>
public sealed class RemoveProjectRepositoryCommandValidator
    : AbstractValidator<RemoveProjectRepositoryCommand>
{
    public RemoveProjectRepositoryCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("ProjectId is required.");

        RuleFor(x => x.RepositoryId)
            .NotEmpty().WithMessage("RepositoryId is required.");
    }
}
