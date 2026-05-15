namespace InfraFlowSculptor.Domain.Common.Constants;

/// <summary>Validates whether a given SKU supports private endpoints for a resource type.</summary>
public static class PrivateEndpointSkuCompatibility
{
    private const string PremiumSku = "Premium";
    private const string FreeSku = "Free";
    private const string DedicatedSku = "Dedicated";

    /// <summary>Returns true if the resource type + SKU combination supports private endpoints.</summary>
    public static bool IsPrivateEndpointCompatible(string resourceType, string? sku)
    {
        return resourceType switch
        {
            "RedisCache" => string.Equals(sku, PremiumSku, StringComparison.OrdinalIgnoreCase),
            "AppConfiguration" => !string.Equals(sku, FreeSku, StringComparison.OrdinalIgnoreCase),
            "ServiceBusNamespace" => string.Equals(sku, PremiumSku, StringComparison.OrdinalIgnoreCase),
            "EventHubNamespace" => sku is PremiumSku or DedicatedSku,
            "ContainerRegistry" => string.Equals(sku, PremiumSku, StringComparison.OrdinalIgnoreCase),
            _ => true
        };
    }
}
