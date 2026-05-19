using FluentValidation;

namespace InfraFlowSculptor.Application.Projects.Commands.VerifyProjectRepositoryConnection;

/// <summary>Validates the <see cref="VerifyProjectRepositoryConnectionCommand"/> before it is handled.</summary>
public sealed class VerifyProjectRepositoryConnectionCommandValidator
    : AbstractValidator<VerifyProjectRepositoryConnectionCommand>
{
    public VerifyProjectRepositoryConnectionCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("ProjectId is required.");

        RuleFor(x => x.ProviderType)
            .NotEmpty().WithMessage("ProviderType is required.");

        RuleFor(x => x.RepositoryUrl)
            .NotEmpty().WithMessage("RepositoryUrl is required.")
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _)).WithMessage("RepositoryUrl must be an absolute URL.");
    }
}