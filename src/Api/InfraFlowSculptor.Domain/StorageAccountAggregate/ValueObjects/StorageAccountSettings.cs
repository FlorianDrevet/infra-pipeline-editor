namespace InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;

/// <summary>
/// Groups the Storage Account-specific configuration settings to keep method signatures concise.
/// </summary>
public record StorageAccountSettings(
    StorageAccountSku Sku,
    StorageAccountKind Kind,
    StorageAccessTier AccessTier,
    bool AllowBlobPublicAccess,
    bool EnableHttpsTrafficOnly,
    StorageAccountTlsVersion MinimumTlsVersion);
