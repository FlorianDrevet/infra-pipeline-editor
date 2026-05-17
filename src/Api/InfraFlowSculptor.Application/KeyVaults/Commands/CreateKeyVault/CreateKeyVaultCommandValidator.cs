using InfraFlowSculptor.Application.Common.Validation;

namespace InfraFlowSculptor.Application.KeyVaults.Commands.CreateKeyVault;

/// <summary>Validates the <see cref="CreateKeyVaultCommand"/> before it is handled.</summary>
public sealed class CreateKeyVaultCommandValidator : CreateResourceCommandValidator<CreateKeyVaultCommand>
{
}
