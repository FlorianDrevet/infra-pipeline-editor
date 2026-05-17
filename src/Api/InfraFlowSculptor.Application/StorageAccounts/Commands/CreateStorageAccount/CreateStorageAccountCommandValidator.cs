using InfraFlowSculptor.Application.Common.Validation;
using InfraFlowSculptor.Application.StorageAccounts.Common;

namespace InfraFlowSculptor.Application.StorageAccounts.Commands.CreateStorageAccount;

/// <summary>Validates the <see cref="CreateStorageAccountCommand"/> before it is handled.</summary>
public sealed class CreateStorageAccountCommandValidator : CreateResourceCommandValidator<CreateStorageAccountCommand>
{
    /// <summary>Initializes validation rules for creating a Storage Account.</summary>
    public CreateStorageAccountCommandValidator()
    {
        this.AddStorageAccountRules();
    }
}
