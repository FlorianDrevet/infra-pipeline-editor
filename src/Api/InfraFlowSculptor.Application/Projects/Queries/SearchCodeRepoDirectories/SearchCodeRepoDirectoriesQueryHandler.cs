using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Projects.Common;

namespace InfraFlowSculptor.Application.Projects.Queries.SearchCodeRepoDirectories;

/// <summary>Handles the <see cref="SearchCodeRepoDirectoriesQuery"/>.</summary>
public sealed class SearchCodeRepoDirectoriesQueryHandler(IGitRepoQueryHelper gitRepoQueryHelper)
    : IQueryHandler<SearchCodeRepoDirectoriesQuery, IReadOnlyList<GitFileResult>>
{
    /// <inheritdoc />
    public async Task<ErrorOr<IReadOnlyList<GitFileResult>>> Handle(
        SearchCodeRepoDirectoriesQuery query, CancellationToken cancellationToken)
    {
        var contextResult = await gitRepoQueryHelper.ResolveAsync(
            query.ProjectId, query.ConfigId, cancellationToken);
        if (contextResult.IsError)
            return contextResult.Errors;

        var ctx = contextResult.Value;
        return await ctx.Provider.SearchDirectoriesAsync(
            ctx.Secret, ctx.Target.Owner, ctx.Target.RepositoryName,
            query.Branch, query.PathPrefix, cancellationToken);
    }
}
