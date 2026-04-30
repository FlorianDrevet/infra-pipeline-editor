using System.ComponentModel;
using System.Text.Json;
using InfraFlowSculptor.Mcp.Common;
using ModelContextProtocol.Server;

namespace InfraFlowSculptor.Mcp.Imports.Resources;

/// <summary>
/// Provides MCP resources for reading stored import previews.
/// </summary>
[McpServerResourceType]
public sealed class ImportPreviewResources
{
    private ImportPreviewResources() { }

    /// <summary>
    /// Returns the full content of a stored import preview.
    /// </summary>
    [McpServerResource(
        UriTemplate = "ifs://imports/{previewId}",
        Name = "Import Preview")]
    [Description("Returns the full content of a stored import preview.")]
    public static string GetImportPreview(
        IImportPreviewService previewService,
        string previewId)
    {
        var preview = previewService.GetPreview(previewId);
        if (preview is null)
        {
            return JsonError("preview_not_found", $"Import preview '{previewId}' not found.");
        }

        return JsonSerializer.Serialize(preview, McpJsonDefaults.SerializerOptions);
    }

    private static string JsonError(string code, string message) =>
        McpJsonDefaults.Error(code, message);
}
