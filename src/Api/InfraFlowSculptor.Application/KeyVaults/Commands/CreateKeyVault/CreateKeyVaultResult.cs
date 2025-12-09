namespace InfraFlowSculptor.Application.KeyVaults.Commands.CreateKeyVault;

public record CreateKeyVaultResult(
    Guid Id,
    string Name,
    string Sku,
    string Location
);