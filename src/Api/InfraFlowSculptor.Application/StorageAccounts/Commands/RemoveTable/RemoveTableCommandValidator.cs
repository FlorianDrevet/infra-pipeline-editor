using FluentValidation;

namespace InfraFlowSculptor.Application.StorageAccounts.Commands.RemoveTable;

/// <summary>Validates the <see cref="RemoveTableCommand"/> before it is handled.</summary>
public sealed class RemoveTableCommandValidator : AbstractValidator<RemoveTableCommand>
{
    /// <summary>Initializes validation rules for removing a storage table.</summary>
    public RemoveTableCommandValidator()
    {
        RuleFor(x => x.StorageAccountId)
            .NotEmpty().WithMessage("StorageAccountId is required.");

        RuleFor(x => x.TableId)
            .NotEmpty().WithMessage("TableId is required.");
    }
}
