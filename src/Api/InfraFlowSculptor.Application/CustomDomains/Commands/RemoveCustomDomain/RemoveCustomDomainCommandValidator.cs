using FluentValidation;

namespace InfraFlowSculptor.Application.CustomDomains.Commands.RemoveCustomDomain;

/// <summary>Validates the <see cref="RemoveCustomDomainCommand"/> before it is handled.</summary>
public sealed class RemoveCustomDomainCommandValidator : AbstractValidator<RemoveCustomDomainCommand>
{
    /// <summary>Initializes validation rules for removing a custom domain.</summary>
    public RemoveCustomDomainCommandValidator()
    {
        RuleFor(x => x.ResourceId)
            .NotEmpty().WithMessage("ResourceId is required.");

        RuleFor(x => x.CustomDomainId)
            .NotEmpty().WithMessage("CustomDomainId is required.");
    }
}