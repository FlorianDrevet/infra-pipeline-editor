using System.Text.RegularExpressions;

namespace InfraFlowSculptor.GenerationCore;

/// <summary>
/// Centralizes Azure Key Vault secret-name validation rules.
/// </summary>
public static partial class KeyVaultSecretNameRules
{
    /// <summary>
    /// The minimum supported Key Vault secret-name length.
    /// </summary>
    public const int MinLength = 1;

    /// <summary>
    /// The maximum supported Key Vault secret-name length.
    /// </summary>
    public const int MaxLength = 127;

    /// <summary>
    /// Shared validation message for invalid Key Vault secret names.
    /// </summary>
    public const string ValidationMessage = "Key Vault secret names must be 1 to 127 characters long and contain only letters, digits, and hyphens.";

    /// <summary>
    /// Returns <c>true</c> when the provided name matches Azure Key Vault secret-name constraints.
    /// </summary>
    public static bool IsValid(string secretName)
    {
        return SecretNamePattern().IsMatch(secretName);
    }

    [GeneratedRegex("^[0-9A-Za-z-]{1,127}$", RegexOptions.Compiled)]
    private static partial Regex SecretNamePattern();
}
