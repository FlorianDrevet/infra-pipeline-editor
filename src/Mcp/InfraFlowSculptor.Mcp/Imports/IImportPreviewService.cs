using InfraFlowSculptor.Mcp.Imports.Models;

namespace InfraFlowSculptor.Mcp.Imports;

/// <summary>Manages in-memory import previews for the MCP session.</summary>
public interface IImportPreviewService
{
    /// <summary>Creates a preview from an ARM JSON template.</summary>
    /// <param name="sourceContent">The raw ARM JSON template content.</param>
    /// <returns>A new <see cref="ImportPreview"/> stored in memory.</returns>
    ImportPreview CreatePreviewFromArm(string sourceContent);

    /// <summary>Retrieves a stored preview by its identifier.</summary>
    /// <param name="previewId">The unique preview identifier.</param>
    /// <returns>The preview if found; otherwise <c>null</c>.</returns>
    ImportPreview? GetPreview(string previewId);

    /// <summary>Removes a preview from storage.</summary>
    /// <param name="previewId">The unique preview identifier.</param>
    /// <returns><c>true</c> if the preview was found and removed; otherwise <c>false</c>.</returns>
    bool RemovePreview(string previewId);
}
