using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using ModelContextProtocol.Server;

namespace InfraFlowSculptor.Mcp.Imports.Resources;

/// <summary>
/// Provides MCP resources for reading stored import previews.
/// </summary>
[McpServerResourceType]
public sealed class ImportPreviewResources
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

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

        return JsonSerializer.Serialize(preview, JsonOptions);
    }

    private static string JsonError(string code, string message) =>
        JsonSerializer.Serialize(new { error = code, message }, JsonOptions);
}
