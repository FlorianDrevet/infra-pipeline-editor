using InfraFlowSculptor.Application.Common.GitRouting;
using InfraFlowSculptor.Application.Common.Interfaces.Services;

namespace InfraFlowSculptor.Application.Projects.Common;

/// <summary>
/// Contains the resolved git provider, PAT secret, and repository target
/// produced by <see cref="IGitRepoQueryHelper"/>.
/// </summary>
/// <param name="Provider">The git provider service for the resolved repository type.</param>
/// <param name="Secret">The Personal Access Token used to authenticate against the git provider.</param>
/// <param name="Target">The resolved repository target (owner, repo name, branch, etc.).</param>
public sealed record GitRepoQueryContext(
    IGitProviderService Provider,
    string Secret,
    ResolvedRepositoryTarget Target);
