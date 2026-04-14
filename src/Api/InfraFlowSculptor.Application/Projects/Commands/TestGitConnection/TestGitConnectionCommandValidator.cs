using FluentValidation;

namespace InfraFlowSculptor.Application.Projects.Commands.TestGitConnection;

/// <summary>Validates the <see cref="TestGitConnectionCommand"/>.</summary>
public sealed class TestGitConnectionCommandValidator
    : AbstractValidator<TestGitConnectionCommand>
{
    public TestGitConnectionCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("ProjectId is required.");
    }
}
