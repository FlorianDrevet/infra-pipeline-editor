namespace InfraFlowSculptor.Contracts.KeyVaults.Requests;

public record CreateKeyVaultRequest(
    Guid ResourceGroupId,
    string Name,
    string Location,
    string Sku
);