using FluentValidation;

namespace InfraFlowSculptor.Application.RedisCaches.Commands.DeleteRedisCache;

/// <summary>Validates the <see cref="DeleteRedisCacheCommand"/>.</summary>
public sealed class DeleteRedisCacheCommandValidator : AbstractValidator<DeleteRedisCacheCommand>
{
    public DeleteRedisCacheCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id is required.");
    }
}
