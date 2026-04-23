using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.SetInfraConfigRepositoryBinding;

/// <summary>
/// Command to set or clear the repository binding of an infrastructure configuration.
/// Pass <c>null</c> in <see cref="RepositoryAlias"/> to clear the binding.
/// </summary>
/// <param name="ProjectId">Identifier of the parent project (used for authorization).</param>
/// <param name="ConfigId">Identifier of the configuration to update.</param>
/// <param name="RepositoryAlias">The target project repository alias, or <c>null</c> to clear the binding.</param>
/// <param name="Branch">Optional branch override.</param>
/// <param name="InfraPath">Optional sub-path inside the repository where Bicep files live.</param>
/// <param name="PipelinePath">Optional sub-path inside the repository where pipeline files live.</param>
public record SetInfraConfigRepositoryBindingCommand(
    ProjectId ProjectId,
    InfrastructureConfigId ConfigId,
    string? RepositoryAlias,
    string? Branch,
    string? InfraPath,
    string? PipelinePath
) : ICommand<Success>;
