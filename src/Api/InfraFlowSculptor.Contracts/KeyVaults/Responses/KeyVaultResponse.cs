namespace InfraFlowSculptor.Contracts.KeyVaults.Responses;

public record KeyVaultResponse(
    Guid Id,
    Guid ResourceGroupId,
    string Name,
    string Location,
    string Sku
);