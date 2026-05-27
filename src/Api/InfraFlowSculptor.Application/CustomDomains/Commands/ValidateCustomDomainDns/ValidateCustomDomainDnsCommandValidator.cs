using FluentValidation;

namespace InfraFlowSculptor.Application.CustomDomains.Commands.ValidateCustomDomainDns;

/// <summary>Validates the <see cref="ValidateCustomDomainDnsCommand"/> before it is handled.</summary>
public sealed class ValidateCustomDomainDnsCommandValidator : AbstractValidator<ValidateCustomDomainDnsCommand>
{
    /// <summary>Initializes a new instance of the <see cref="ValidateCustomDomainDnsCommandValidator"/> class.</summary>
    public ValidateCustomDomainDnsCommandValidator()
    {
        RuleFor(x => x.ResourceId)
            .NotNull()
            .WithMessage("Resource ID is required.");

        RuleFor(x => x.CustomDomainId)
            .NotNull()
            .WithMessage("Custom domain ID is required.");
    }
}
