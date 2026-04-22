using FluentValidation;

namespace InfraFlowSculptor.Application.CustomDomains.Commands.AddCustomDomain;

/// <summary>Validates the <see cref="AddCustomDomainCommand"/> before it is handled.</summary>
public sealed class AddCustomDomainCommandValidator : AbstractValidator<AddCustomDomainCommand>
{
    private const string FqdnPattern = @"^(?!-)[a-zA-Z0-9-]{1,63}(?<!-)(\.[a-zA-Z0-9-]{1,63})*\.[a-zA-Z]{2,}$";

    /// <summary>Initializes a new instance of the <see cref="AddCustomDomainCommandValidator"/> class.</summary>
    public AddCustomDomainCommandValidator()
    {
        RuleFor(x => x.ResourceId)
            .NotNull()
            .WithMessage("Resource ID is required.");

        RuleFor(x => x.EnvironmentName)
            .NotEmpty()
            .WithMessage("Environment name is required.")
            .MaximumLength(100)
            .WithMessage("Environment name must not exceed 100 characters.");

        RuleFor(x => x.DomainName)
            .NotEmpty()
            .WithMessage("Domain name is required.")
            .MaximumLength(253)
            .WithMessage("Domain name must not exceed 253 characters.")
            .Matches(FqdnPattern)
            .WithMessage("Domain name must be a valid fully qualified domain name (e.g. api.example.com).");

        RuleFor(x => x.BindingType)
            .Must(bt => bt is "SniEnabled" or "Disabled")
            .WithMessage("Binding type must be either 'SniEnabled' or 'Disabled'.");
    }
}
