using FluentValidation;
using InfraFlowSculptor.Application.Common.Validation;

namespace InfraFlowSculptor.Application.RedisCaches.Commands.CreateRedisCache;

/// <summary>
/// Validates the <see cref="CreateRedisCacheCommand"/> before it is handled.
/// </summary>
public sealed class CreateRedisCacheCommandValidator : CreateResourceCommandValidator<CreateRedisCacheCommand>
{
    /// <summary>Initializes validation rules for creating a Redis Cache.</summary>
    public CreateRedisCacheCommandValidator()
    {
        RuleFor(x => x.EnableAadAuth)
            .Equal(true)
            .When(x => x.DisableAccessKeyAuthentication)
            .WithMessage("AAD authentication must be enabled when access key authentication is disabled.");
    }
}
