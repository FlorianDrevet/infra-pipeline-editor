using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

/// <summary>Strongly-typed identifier for <see cref="Entities.GitRepositoryConfiguration"/>.</summary>
public sealed class GitRepositoryConfigurationId(Guid value) : Id<GitRepositoryConfigurationId>(value);
