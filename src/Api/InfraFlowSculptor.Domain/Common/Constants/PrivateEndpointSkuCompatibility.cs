namespace InfraFlowSculptor.Domain.Common.Constants;

/// <summary>Validates whether a given SKU supports private endpoints for a resource type.</summary>
public static class PrivateEndpointSkuCompatibility
{
    /// <summary>Returns true if the resource type + SKU combination supports private endpoints.</summary>
    public static bool IsPrivateEndpointCompatible(string resourceType, string? sku)
    {
        return resourceType switch
        {
            "RedisCache" => string.Equals(sku, "Premium", StringComparison.OrdinalIgnoreCase),
            "AppConfiguration" => !string.Equals(sku, "Free", StringComparison.OrdinalIgnoreCase),
            "ServiceBusNamespace" => string.Equals(sku, "Premium", StringComparison.OrdinalIgnoreCase),
            "EventHubNamespace" => sku is "Premium" or "Dedicated",
            "ContainerRegistry" => string.Equals(sku, "Premium", StringComparison.OrdinalIgnoreCase),
            _ => true
        };
    }
}
