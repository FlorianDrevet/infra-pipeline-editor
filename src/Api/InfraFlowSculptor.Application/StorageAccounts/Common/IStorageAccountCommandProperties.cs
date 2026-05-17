namespace InfraFlowSculptor.Application.StorageAccounts.Common;

/// <summary>
/// Shared properties between <c>CreateStorageAccountCommand</c> and <c>UpdateStorageAccountCommand</c>,
/// used by <see cref="StorageAccountValidationRules"/> to register common validation rules.
/// </summary>
public interface IStorageAccountCommandProperties
{
    /// <summary>The storage account kind (e.g. StorageV2, BlobStorage).</summary>
    string Kind { get; }

    /// <summary>The storage access tier (e.g. Hot, Cool).</summary>
    string AccessTier { get; }

    /// <summary>The minimum TLS version for the storage account.</summary>
    string MinimumTlsVersion { get; }
}
