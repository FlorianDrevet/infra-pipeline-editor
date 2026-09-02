using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.Projects.Requests;

/// <summary>Describes one configuration-owned repository push target.</summary>
/// <param name="RepositoryId">The configuration repository identifier.</param>
/// <param name="BranchName">The branch to create or update.</param>
/// <param name="CommitMessage">The commit message.</param>
public sealed record PushProjectMultiRepoRepositoryRequest(
    [property: Required] Guid RepositoryId,
    [property: Required, StringLength(200)] string BranchName,
    [property: Required, StringLength(500)] string CommitMessage);