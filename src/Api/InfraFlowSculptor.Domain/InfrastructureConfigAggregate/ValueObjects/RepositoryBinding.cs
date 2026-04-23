using ErrorOr;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

/// <summary>
/// Owned value object describing how an <see cref="InfrastructureConfig"/> binds to a
/// project-level <see cref="ProjectAggregate.Entities.ProjectRepository"/>.
/// The <see cref="Alias"/> is a logical reference validated at the application layer
/// (not enforced by a SQL foreign key) so that bindings can survive repository renames
/// performed in a single transaction.
/// </summary>
public sealed class RepositoryBinding : ValueObject
{
    /// <summary>Gets the alias of the target project repository.</summary>
    public RepositoryAlias Alias { get; private set; } = null!;

    /// <summary>Gets the override branch. When <c>null</c>, the repository default branch is used.</summary>
    public string? Branch { get; private set; }

    /// <summary>Gets the optional sub-path inside the repository where Bicep files live.</summary>
    public string? InfraPath { get; private set; }

    /// <summary>Gets the optional sub-path inside the repository where pipeline files live.</summary>
    public string? PipelinePath { get; private set; }

    /// <summary>EF Core constructor.</summary>
    private RepositoryBinding() { }

    private RepositoryBinding(RepositoryAlias alias, string? branch, string? infraPath, string? pipelinePath)
    {
        Alias = alias;
        Branch = branch;
        InfraPath = infraPath;
        PipelinePath = pipelinePath;
    }

    /// <summary>
    /// Creates a new <see cref="RepositoryBinding"/>. Paths are normalized (trimmed of surrounding slashes).
    /// If a branch is provided it must be non-empty.
    /// </summary>
    /// <param name="alias">The target repository alias.</param>
    /// <param name="branch">Optional branch override.</param>
    /// <param name="infraPath">Optional infrastructure sub-path.</param>
    /// <param name="pipelinePath">Optional pipeline sub-path.</param>
    /// <returns>The created binding or a validation error.</returns>
    public static ErrorOr<RepositoryBinding> Create(
        RepositoryAlias alias,
        string? branch,
        string? infraPath,
        string? pipelinePath)
    {
        string? normalizedBranch = null;
        if (branch is not null)
        {
            var trimmed = branch.Trim();
            if (trimmed.Length == 0)
                return Errors.GitRepository.InvalidRepositoryUrl();

            normalizedBranch = trimmed;
        }

        return new RepositoryBinding(
            alias,
            normalizedBranch,
            NormalizePath(infraPath),
            NormalizePath(pipelinePath));
    }

    private static string? NormalizePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return null;

        return path.Trim().Trim('/');
    }

    /// <inheritdoc />
    public override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Alias;
        yield return Branch;
        yield return InfraPath;
        yield return PipelinePath;
    }
}
