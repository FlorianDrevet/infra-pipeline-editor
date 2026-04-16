namespace InfraFlowSculptor.BicepGeneration.Helpers;

/// <summary>
/// Provides the canonical camelCase identifier normalization strategy for Bicep generation.
/// All Bicep identifiers (resource symbolic names, object keys, parameter names) produced
/// by this project use camelCase: hyphens, underscores, and spaces are treated as word
/// separators, the first word is fully lowercased, and each subsequent word is title-cased.
/// Example: <c>"my-rg-prod"</c> → <c>"myRgProd"</c>.
/// </summary>
internal static class BicepIdentifierHelper
{
    /// <summary>
    /// Converts an Azure resource name (e.g. <c>"my-rg-prod"</c>) to a valid camelCase Bicep
    /// identifier (e.g. <c>"myRgProd"</c>). Hyphens, underscores, and spaces are treated as
    /// word separators. Returns <c>"resource"</c> when the input is empty or produces no parts.
    /// </summary>
    internal static string ToBicepIdentifier(string name)
    {
        if (string.IsNullOrEmpty(name)) return "resource";
        var parts = name.Split(['-', '_', ' '], StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return "resource";
        var sb = new System.Text.StringBuilder(parts[0].ToLowerInvariant());
        foreach (var part in parts.Skip(1))
        {
            if (part.Length > 0)
                sb.Append(char.ToUpperInvariant(part[0])).Append(part[1..].ToLowerInvariant());
        }
        return sb.ToString();
    }
}
