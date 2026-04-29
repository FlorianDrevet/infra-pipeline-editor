using FluentValidation;

namespace InfraFlowSculptor.Application.PersonalAccessTokens.Commands.CreatePersonalAccessToken;

/// <summary>
/// Validates the <see cref="CreatePersonalAccessTokenCommand"/> before it is handled.
/// </summary>
public sealed class CreatePersonalAccessTokenCommandValidator
    : AbstractValidator<CreatePersonalAccessTokenCommand>
{
    /// <summary>Maximum allowed length for the token name.</summary>
    private const int MaxNameLength = 100;

    public CreatePersonalAccessTokenCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(MaxNameLength).WithMessage($"Name must not exceed {MaxNameLength} characters.");

        RuleFor(x => x.ExpiresAt)
            .GreaterThan(DateTime.UtcNow).WithMessage("Expiration date must be in the future.")
            .When(x => x.ExpiresAt.HasValue);
    }
}
