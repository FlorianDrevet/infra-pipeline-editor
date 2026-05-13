using FluentValidation;

namespace InfraFlowSculptor.Application.PersonalAccessTokens.Commands.RevokePersonalAccessToken;

/// <summary>Validates the <see cref="RevokePersonalAccessTokenCommand"/> before it is handled.</summary>
public sealed class RevokePersonalAccessTokenCommandValidator
    : AbstractValidator<RevokePersonalAccessTokenCommand>
{
    /// <summary>Initializes validation rules for <see cref="RevokePersonalAccessTokenCommand"/>.</summary>
    public RevokePersonalAccessTokenCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required.");
    }
}