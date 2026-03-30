using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Commands.GenerateProjectBicep;

/// <summary>Command to generate Bicep files for an entire project in mono-repo mode.</summary>
public record GenerateProjectBicepCommand(
    ProjectId ProjectId
) : ICommand<GenerateProjectBicepResult>;

/// <summary>Result of mono-repo Bicep generation, containing URIs organized by Common and per-config folders.</summary>
/// <param name="CommonFileUris">Shared files under the Common/ directory (types.bicep, functions.bicep, modules/...).</param>
/// <param name="ConfigFileUris">Per-configuration files keyed by config name, then relative path.</param>
public record GenerateProjectBicepResult(
    IReadOnlyDictionary<string, Uri> CommonFileUris,
    IReadOnlyDictionary<string, IReadOnlyDictionary<string, Uri>> ConfigFileUris);
