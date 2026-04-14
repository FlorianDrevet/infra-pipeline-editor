namespace InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

/// <summary>Determines how a secret static app setting value is assigned.</summary>
public enum SecretValueAssignment
{
    /// <summary>Value provided at deployment time via .bicepparam file.</summary>
    ViaBicepparam,

    /// <summary>Value entered directly in Azure Key Vault (not managed by Bicep).</summary>
    DirectInKeyVault
}
