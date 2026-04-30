namespace InfraFlowSculptor.GenerationCore;

/// <summary>
/// Canonical default values for Azure resource properties, shared across generation engines and import tooling.
/// </summary>
public static class AzureResourceDefaults
{
    /// <summary>Default minimum TLS version for Azure resources that support TLS configuration.</summary>
    public const string MinimumTlsVersion = "1.2";

    /// <summary>Default SQL Server version (SQL Server 2014+).</summary>
    public const string SqlServerVersion = "12.0";

    /// <summary>Default SQL Server administrator login name.</summary>
    public const string SqlServerAdministratorLogin = "sqladmin";

    /// <summary>Default Storage Account kind.</summary>
    public const string StorageAccountKind = "StorageV2";

    /// <summary>Default Storage Account access tier.</summary>
    public const string StorageAccountAccessTier = "Hot";

    /// <summary>Default minimum TLS version label used by Storage Account / Web App API surfaces (uses underscore-prefixed form).</summary>
    public const string MinimumTlsVersionLabel = "TLS1_2";

    /// <summary>Default deployment mode for Web App and Function App container/code deployments.</summary>
    public const string AppServiceDeploymentMode = "Zip";

    /// <summary>Default SQL Database collation.</summary>
    public const string SqlDatabaseCollation = "SQL_Latin1_General_CP1_CI_AS";
}
