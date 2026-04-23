using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Commands.SetProjectCommonsStrategy;

/// <summary>
/// Command to update the commons strategy of a project. In V1 only <c>DuplicatePerRepo</c>
/// is accepted; other values are rejected by the domain. Other strategies will be unlocked
/// in a future iteration (V3).
/// </summary>
/// <param name="ProjectId">Identifier of the target project.</param>
/// <param name="Strategy">Strategy name. V1 only accepts <c>DuplicatePerRepo</c>.</param>
public record SetProjectCommonsStrategyCommand(
    ProjectId ProjectId,
    string Strategy
) : ICommand<Success>;
