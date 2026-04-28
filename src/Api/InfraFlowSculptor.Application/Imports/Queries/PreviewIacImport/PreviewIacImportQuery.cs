using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Imports.Common;

namespace InfraFlowSculptor.Application.Imports.Queries.PreviewIacImport;

/// <summary>
/// Represents a request to preview an IaC import source.
/// </summary>
/// <param name="SourceFormat">The source format identifier.</param>
/// <param name="SourceContent">The raw source content.</param>
public sealed record PreviewIacImportQuery(
    string SourceFormat,
    string SourceContent) : IQuery<ImportPreviewAnalysisResult>;