using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.SetInfraConfigTags;

/// <summary>Command to replace all configuration-level tags.</summary>
public sealed record SetInfraConfigTagsCommand(
    Guid InfraConfigId,
    IReadOnlyCollection<(string Name, string Value)> Tags) : ICommand<Updated>;
