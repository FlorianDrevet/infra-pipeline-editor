using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Common.Interfaces.Services;

/// <summary>
/// Resolves the correct <see cref="IGitProviderService"/> based on the provider type.
/// </summary>
public interface IGitProviderFactory
{
    /// <summary>
    /// Returns the Git provider service matching the given type.
    /// </summary>
    IGitProviderService Create(GitProviderType providerType);
}
