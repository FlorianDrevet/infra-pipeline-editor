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
}
