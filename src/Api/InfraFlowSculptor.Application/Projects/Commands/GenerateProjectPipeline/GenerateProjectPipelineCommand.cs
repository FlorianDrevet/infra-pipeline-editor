using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Commands.GenerateProjectPipeline;

/// <summary>Command to generate pipeline YAML files for an entire project in mono-repo mode.</summary>
public record GenerateProjectPipelineCommand(
    ProjectId ProjectId
) : ICommand<GenerateProjectPipelineResult>;

/// <summary>Result of mono-repo pipeline generation, containing URIs organized by common and per-config folders.</summary>
/// <param name="CommonFileUris">Union of infra and app shared templates (backward compatibility).</param>
/// <param name="ConfigFileUris">Union of infra and app per-config files (backward compatibility).</param>
/// <param name="InfraCommonFileUris">Shared template files routed to the infrastructure repository.</param>
/// <param name="AppCommonFileUris">Shared application pipeline templates routed to the application-code repository.</param>
/// <param name="InfraConfigFileUris">Per-configuration files routed to the infrastructure repository.</param>
/// <param name="AppConfigFileUris">Per-configuration files (per-resource wrappers under <c>apps/</c>) routed to the application-code repository.</param>
public record GenerateProjectPipelineResult(
    IReadOnlyDictionary<string, Uri> CommonFileUris,
    IReadOnlyDictionary<string, IReadOnlyDictionary<string, Uri>> ConfigFileUris,
    IReadOnlyDictionary<string, Uri> InfraCommonFileUris,
    IReadOnlyDictionary<string, Uri> AppCommonFileUris,
    IReadOnlyDictionary<string, IReadOnlyDictionary<string, Uri>> InfraConfigFileUris,
    IReadOnlyDictionary<string, IReadOnlyDictionary<string, Uri>> AppConfigFileUris);
