using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Imports.Common;

namespace InfraFlowSculptor.Application.Imports.Queries.PreviewIacImport;

/// <summary>
/// Handles <see cref="PreviewIacImportQuery"/> requests.
/// </summary>
public sealed class PreviewIacImportQueryHandler(IImportPreviewAnalyzer analyzer)
    : IQueryHandler<PreviewIacImportQuery, ImportPreviewAnalysisResult>
{
    private const string ArmJsonSourceFormat = "arm-json";

    /// <inheritdoc />
    public Task<ErrorOr<ImportPreviewAnalysisResult>> Handle(
        PreviewIacImportQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (!string.Equals(query.SourceFormat, ArmJsonSourceFormat, StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult<ErrorOr<ImportPreviewAnalysisResult>>(new ImportPreviewAnalysisResult
            {
                SourceFormat = query.SourceFormat,
                Summary = $"Source format '{query.SourceFormat}' is not supported in V1. Supported formats: arm-json.",
            });
        }

        return Task.FromResult<ErrorOr<ImportPreviewAnalysisResult>>(analyzer.AnalyzeArmTemplate(query.SourceContent));
    }
}