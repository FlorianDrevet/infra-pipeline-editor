using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Imports.Common;
using InfraFlowSculptor.Application.Projects.Commands.CreateProjectWithSetup;

namespace InfraFlowSculptor.Application.Imports.Commands.ApplyImportPreview;

/// <summary>
/// Represents a request to apply an import preview to a newly created project.
/// </summary>
/// <param name="ProjectName">The name of the project to create.</param>
/// <param name="LayoutPreset">The layout preset for the new project.</param>
/// <param name="Preview">The normalized preview payload to apply.</param>
/// <param name="Environments">The optional environment definitions for the new project.</param>
/// <param name="ResourceFilter">The optional source resource names to include.</param>
public sealed record ApplyImportPreviewCommand(
    string ProjectName,
    string LayoutPreset,
    ImportPreviewAnalysisResult Preview,
    IReadOnlyList<EnvironmentSetupItem>? Environments = null,
    IReadOnlyList<string>? ResourceFilter = null)
    : ICommand<ApplyImportPreviewResult>;