using FluentValidation;

namespace InfraFlowSculptor.Application.RedisCaches.Commands.UpdateRedisCache;

/// <summary>
/// Validates the <see cref="UpdateRedisCacheCommand"/> before it is handled.
/// </summary>
public sealed class UpdateRedisCacheCommandValidator : AbstractValidator<UpdateRedisCacheCommand>
{
    /// <summary>Initializes validation rules for updating a Redis Cache.</summary>
    public UpdateRedisCacheCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.");

        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required.");

        RuleFor(x => x.EnableAadAuth)
            .Equal(true)
            .When(x => x.DisableAccessKeyAuthentication)
            .WithMessage("AAD authentication must be enabled when access key authentication is disabled.");
    }
}
