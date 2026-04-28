using System.Collections.Concurrent;
using InfraFlowSculptor.Application.Imports.Common;
using InfraFlowSculptor.GenerationCore;
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
            ProjectDefinition = new ImportedProjectDefinition
            {
                SourceFormat = analysis.SourceFormat,
                Resources = analysis.Resources.Select(MapResource).ToList(),
                Dependencies = analysis.Dependencies.Select(MapDependency).ToList(),
                Metadata = new Dictionary<string, string>(analysis.Metadata),
            },
            Gaps = analysis.Gaps.Select(MapGap).ToList(),
            UnsupportedResources = analysis.UnsupportedResources.ToList(),
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

    private static string GeneratePreviewId() =>
        "preview_" + Guid.NewGuid().ToString("N")[..8];

    private static ImportedResourceDefinition MapResource(ImportedResourceAnalysisResult resource)
    {
        return new ImportedResourceDefinition
        {
            SourceType = resource.SourceType,
            SourceName = resource.SourceName,
            MappedResourceType = resource.MappedResourceType,
            MappedName = resource.MappedName,
            Confidence = MapConfidence(resource.Confidence),
            ExtractedProperties = new Dictionary<string, object?>(resource.ExtractedProperties),
            UnmappedProperties = resource.UnmappedProperties.ToList(),
        };
    }

    private static ImportedDependency MapDependency(ImportedDependencyAnalysisResult dependency)
    {
        return new ImportedDependency(
            dependency.FromResourceName,
            dependency.ToResourceName,
            dependency.DependencyType);
    }

    private static ImportGap MapGap(ImportPreviewGapResult gap)
    {
        return new ImportGap
        {
            Severity = MapSeverity(gap.Severity),
            Category = gap.Category,
            Message = gap.Message,
            SourceResourceName = gap.SourceResourceName,
        };
    }

    private static MappingConfidence MapConfidence(ImportPreviewMappingConfidence confidence)
    {
        return confidence switch
        {
            ImportPreviewMappingConfidence.High => MappingConfidence.High,
            ImportPreviewMappingConfidence.Medium => MappingConfidence.Medium,
            _ => MappingConfidence.Low,
        };
    }

    private static ImportGapSeverity MapSeverity(ImportPreviewGapSeverity severity)
    {
        return severity switch
        {
            ImportPreviewGapSeverity.Info => ImportGapSeverity.Info,
            ImportPreviewGapSeverity.Warning => ImportGapSeverity.Warning,
            _ => ImportGapSeverity.Error,
        };
    }
}
