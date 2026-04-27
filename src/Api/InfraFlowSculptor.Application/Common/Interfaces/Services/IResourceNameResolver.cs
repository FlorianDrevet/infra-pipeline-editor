using ErrorOr;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Common.Interfaces.Services;

/// <summary>
/// Resolves the final generated resource name per environment by applying the project/config
/// naming templates (with placeholder substitution).
/// </summary>
public interface IResourceNameResolver
{
    /// <summary>
    /// Returns the resolved names for every environment defined on the project.
    /// </summary>
    /// <param name="projectId">Project owning the environments and naming templates.</param>
    /// <param name="configId">Optional infrastructure config whose templates may override the project templates.</param>
    /// <param name="resourceType">Friendly resource type (e.g. <c>"ContainerRegistry"</c>).</param>
    /// <param name="rawName">User-entered raw name used to substitute the <c>{name}</c> placeholder.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<ErrorOr<IReadOnlyList<ResolvedResourceName>>> ResolveAsync(
        ProjectId projectId,
        InfrastructureConfigId? configId,
        string resourceType,
        string rawName,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// A resolved name for one environment.
/// </summary>
/// <param name="EnvironmentName">Display name of the environment (e.g. "Development").</param>
/// <param name="EnvironmentShortName">Short identifier of the environment (e.g. "dev").</param>
/// <param name="SubscriptionId">Azure subscription identifier of the environment.</param>
/// <param name="GeneratedName">Final resource name produced by applying the template.</param>
/// <param name="AppliedTemplate">The naming template string that was applied.</param>
public sealed record ResolvedResourceName(
    string EnvironmentName,
    string EnvironmentShortName,
    string SubscriptionId,
    string GeneratedName,
    string AppliedTemplate);
