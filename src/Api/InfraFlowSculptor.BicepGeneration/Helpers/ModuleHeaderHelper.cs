using System.Text;

namespace InfraFlowSculptor.BicepGeneration.Helpers;

/// <summary>
/// Generates and applies formatted header comments for Bicep module files.
/// </summary>
internal static class ModuleHeaderHelper
{
    /// <summary>
    /// Generates a formatted module header comment with description and documentation link.
    /// </summary>
    internal static string GenerateModuleHeader(
        string resourceTypeName,
        string moduleFileName)
    {
        var displayName = ResourceTypeMetadata.GetResourceTypeDisplayName(resourceTypeName);
        var docUrl = ResourceTypeMetadata.GetResourceTypeDocumentationUrl(resourceTypeName);

        var sb = new StringBuilder();
        sb.AppendLine("// =======================================================================");
        sb.AppendLine($"// {displayName} Module");
        sb.AppendLine("// -----------------------------------------------------------------------");
        sb.AppendLine($"// Module: {moduleFileName}");
        sb.AppendLine($"// Description: Deploys an Azure {displayName} resource");
        if (!string.IsNullOrEmpty(docUrl))
        {
            sb.AppendLine($"// See: {docUrl}");
        }
        sb.AppendLine("// =======================================================================");
        sb.AppendLine();

        return sb.ToString();
    }

    /// <summary>
    /// Adds a formatted header to the module Bicep content.
    /// </summary>
    internal static string AddModuleHeader(
        string resourceTypeName,
        string moduleFileName,
        string bicepContent)
    {
        var header = GenerateModuleHeader(resourceTypeName, moduleFileName);
        return header + bicepContent;
    }

    internal static string MergeTypesContent(string existingContent, string additionalContent)
    {
        if (string.IsNullOrWhiteSpace(additionalContent) || existingContent.Contains(additionalContent, StringComparison.Ordinal))
        {
            return existingContent;
        }

        return $"{existingContent.TrimEnd()}\n\n{additionalContent.Trim()}\n";
    }
}
