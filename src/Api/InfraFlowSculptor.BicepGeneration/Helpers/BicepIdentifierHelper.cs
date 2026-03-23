namespace InfraFlowSculptor.BicepGeneration.Helpers;

internal static class BicepIdentifierHelper
{
    /// <summary>
    /// Converts an Azure resource name (e.g. "my-rg-prod") to a valid camelCase Bicep identifier
    /// (e.g. "myRgProd"). Hyphens, underscores, and spaces are treated as word separators.
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
