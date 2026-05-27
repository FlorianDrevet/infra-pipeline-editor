using FluentValidation;

namespace InfraFlowSculptor.Application.Projects.Commands.TestProjectRepositoryConnection;

/// <summary>
/// Validates the <see cref="TestProjectRepositoryConnectionCommand"/> before it is handled.
/// </summary>
public sealed class TestProjectRepositoryConnectionCommandValidator
    : AbstractValidator<TestProjectRepositoryConnectionCommand>
{
    /// <summary>
    /// Initializes the validator.
    /// </summary>
    public TestProjectRepositoryConnectionCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("ProjectId is required.");

        RuleFor(x => x.RepositoryId)
            .NotEmpty().WithMessage("RepositoryId is required.");
    }
}