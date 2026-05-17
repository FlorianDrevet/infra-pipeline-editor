using FluentValidation;

namespace InfraFlowSculptor.Application.StorageAccounts.Commands.RemoveQueue;

/// <summary>Validates the <see cref="RemoveQueueCommand"/> before it is handled.</summary>
public sealed class RemoveQueueCommandValidator : AbstractValidator<RemoveQueueCommand>
{
    /// <summary>Initializes validation rules for removing a storage queue.</summary>
    public RemoveQueueCommandValidator()
    {
        RuleFor(x => x.StorageAccountId)
            .NotEmpty().WithMessage("StorageAccountId is required.");

        RuleFor(x => x.QueueId)
            .NotEmpty().WithMessage("QueueId is required.");
    }
}
