using InfraFlowSculptor.Application.InfrastructureConfig.ReadModels;

namespace InfraFlowSculptor.Application.Common.Interfaces.Services;

/// <summary>
/// Resolves the folder name used for generated application pipeline artifacts.
/// </summary>
public interface IApplicationFolderNameResolver
{
    /// <summary>
    /// Resolves the folder name for the given compute resource.
    /// </summary>
    Task<string> ResolveAsync(AzureResourceReadModel resource, CancellationToken cancellationToken = default);
}
