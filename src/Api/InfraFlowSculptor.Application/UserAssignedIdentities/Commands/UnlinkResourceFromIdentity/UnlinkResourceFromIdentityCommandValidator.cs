using FluentValidation;

namespace InfraFlowSculptor.Application.UserAssignedIdentities.Commands.UnlinkResourceFromIdentity;

/// <summary>Validates the <see cref="UnlinkResourceFromIdentityCommand"/> before it is handled.</summary>
public sealed class UnlinkResourceFromIdentityCommandValidator : AbstractValidator<UnlinkResourceFromIdentityCommand>
{
    /// <summary>Initializes validation rules for unlinking a resource from an identity.</summary>
    public UnlinkResourceFromIdentityCommandValidator()
    {
        RuleFor(x => x.IdentityId)
            .NotEmpty().WithMessage("IdentityId is required.");

        RuleFor(x => x.SourceResourceId)
            .NotEmpty().WithMessage("SourceResourceId is required.");
    }
}
