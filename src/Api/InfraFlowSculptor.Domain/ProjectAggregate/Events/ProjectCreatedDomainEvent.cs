using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.ProjectAggregate.Events;

/// <summary>
/// Represents the domain event raised when a project is created.
/// </summary>
/// <param name="ProjectId">The identifier of the created project.</param>
public sealed record ProjectCreatedDomainEvent(ProjectId ProjectId) : IDomainEvent;