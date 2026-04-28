namespace InfraFlowSculptor.Application.Imports.Common;

/// <summary>
/// Analyzes IaC source content and returns a normalized import preview analysis.
/// </summary>
public interface IImportPreviewAnalyzer
{
    /// <summary>
    /// Analyzes an ARM JSON template and returns a normalized import preview analysis.
    /// </summary>
    /// <param name="sourceContent">The raw ARM JSON template content.</param>
    /// <returns>The normalized import preview analysis.</returns>
    ImportPreviewAnalysisResult AnalyzeArmTemplate(string sourceContent);
}