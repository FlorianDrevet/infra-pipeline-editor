using FluentValidation;

namespace InfraFlowSculptor.Application.StorageAccounts.Commands.DeleteStorageAccount;

/// <summary>Validates the <see cref="DeleteStorageAccountCommand"/>.</summary>
public sealed class DeleteStorageAccountCommandValidator : AbstractValidator<DeleteStorageAccountCommand>
{
    public DeleteStorageAccountCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id is required.");
    }
}
