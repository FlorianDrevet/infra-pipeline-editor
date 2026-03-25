using FluentValidation;

namespace InfraFlowSculptor.Application.RedisCaches.Commands.CreateRedisCache;

/// <summary>
/// Validates the <see cref="CreateRedisCacheCommand"/> before it is handled.
/// </summary>
public sealed class CreateRedisCacheCommandValidator : AbstractValidator<CreateRedisCacheCommand>
{
    /// <summary>Initializes validation rules for creating a Redis Cache.</summary>
    public CreateRedisCacheCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.");

        RuleFor(x => x.ResourceGroupId)
            .NotEmpty().WithMessage("ResourceGroupId is required.");

        RuleFor(x => x.EnableAadAuth)
            .Equal(true)
            .When(x => x.DisableAccessKeyAuthentication)
            .WithMessage("AAD authentication must be enabled when access key authentication is disabled.");
    }
}
