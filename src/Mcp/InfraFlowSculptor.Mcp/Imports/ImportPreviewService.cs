using System.Collections.Concurrent;
using InfraFlowSculptor.Application.Imports.Common.Analysis;
using InfraFlowSculptor.Mcp.Imports.Models;

namespace InfraFlowSculptor.Mcp.Imports;

/// <summary>In-memory implementation of <see cref="IImportPreviewService"/> for the MCP session.</summary>
public sealed class ImportPreviewService : IImportPreviewService
{
    private readonly ConcurrentDictionary<string, ImportPreview> _previews = new();
    private readonly IImportPreviewAnalyzer _analyzer;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImportPreviewService"/> class.
    /// </summary>
    /// <param name="analyzer">The shared import preview analyzer.</param>
    public ImportPreviewService(IImportPreviewAnalyzer analyzer)
    {
        _analyzer = analyzer;
    }

    /// <inheritdoc />
    public ImportPreview CreatePreviewFromArm(string sourceContent)
    {
        var analysis = _analyzer.AnalyzeArmTemplate(sourceContent);

        var id = GeneratePreviewId();
        var preview = new ImportPreview
        {
            PreviewId = id,
            Analysis = analysis,
        };

        _previews[id] = preview;
        return preview;
    }

    /// <inheritdoc />
    public ImportPreview? GetPreview(string previewId)
    {
        return _previews.TryGetValue(previewId, out var preview) ? preview : null;
    }

    /// <inheritdoc />
    public bool RemovePreview(string previewId)
    {
        return _previews.TryRemove(previewId, out _);
    }

    /// <inheritdoc />
    public int EvictExpired(TimeSpan maxAge)
    {
        var cutoff = DateTime.UtcNow - maxAge;
        var expired = _previews
            .Where(kvp => kvp.Value.CreatedAtUtc < cutoff)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in expired)
            _previews.TryRemove(key, out _);

        return expired.Count;
    }

    private static string GeneratePreviewId() =>
        "preview_" + Guid.NewGuid().ToString("N")[..8];
}
