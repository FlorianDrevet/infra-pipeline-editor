using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Imports.Common.Analysis;
using InfraFlowSculptor.Application.Imports.Common.Constants;

namespace InfraFlowSculptor.Application.Imports.Queries.PreviewIacImport;

/// <summary>
/// Handles <see cref="PreviewIacImportQuery"/> requests.
/// </summary>
public sealed class PreviewIacImportQueryHandler(IImportPreviewAnalyzer analyzer)
    : IQueryHandler<PreviewIacImportQuery, ImportPreviewAnalysisResult>
{
    /// <inheritdoc />
    public Task<ErrorOr<ImportPreviewAnalysisResult>> Handle(
        PreviewIacImportQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (!string.Equals(query.SourceFormat, IacSourceFormat.ArmJson, StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult<ErrorOr<ImportPreviewAnalysisResult>>(new ImportPreviewAnalysisResult
            {
                SourceFormat = query.SourceFormat,
                Summary = $"Source format '{query.SourceFormat}' is not supported in V1. Supported formats: {IacSourceFormat.ArmJson}.",
            });
        }

        return Task.FromResult<ErrorOr<ImportPreviewAnalysisResult>>(analyzer.AnalyzeArmTemplate(query.SourceContent));
    }
}