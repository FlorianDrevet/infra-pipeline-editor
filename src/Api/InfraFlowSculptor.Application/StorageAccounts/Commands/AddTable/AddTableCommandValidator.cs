using FluentValidation;

namespace InfraFlowSculptor.Application.StorageAccounts.Commands.AddTable;

/// <summary>Validates the <see cref="AddTableCommand"/> before it is handled.</summary>
public sealed class AddTableCommandValidator : AbstractValidator<AddTableCommand>
{
    /// <summary>Initializes validation rules for adding a storage table.</summary>
    public AddTableCommandValidator()
    {
        RuleFor(x => x.StorageAccountId)
            .NotEmpty().WithMessage("StorageAccountId is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.");
    }
}
