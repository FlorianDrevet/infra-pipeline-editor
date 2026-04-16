using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;

namespace InfraFlowSculptor.Application.Projects.Commands.SetProjectTags;

/// <summary>Command to replace all project-level tags.</summary>
public sealed record SetProjectTagsCommand(
    Guid ProjectId,
    IReadOnlyCollection<(string Name, string Value)> Tags) : ICommand<Updated>;
