using FluentValidation;

namespace InfraFlowSculptor.Application.StorageAccounts.Commands.AddQueue;

/// <summary>Validates the <see cref="AddQueueCommand"/> before it is handled.</summary>
public sealed class AddQueueCommandValidator : AbstractValidator<AddQueueCommand>
{
    /// <summary>Initializes validation rules for adding a storage queue.</summary>
    public AddQueueCommandValidator()
    {
        RuleFor(x => x.StorageAccountId)
            .NotEmpty().WithMessage("StorageAccountId is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.");
    }
}
