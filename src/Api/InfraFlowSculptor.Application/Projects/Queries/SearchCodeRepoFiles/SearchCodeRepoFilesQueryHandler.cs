using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Projects.Common;

namespace InfraFlowSculptor.Application.Projects.Queries.SearchCodeRepoFiles;

/// <summary>Handles the <see cref="SearchCodeRepoFilesQuery"/>.</summary>
public sealed class SearchCodeRepoFilesQueryHandler(IGitRepoQueryHelper gitRepoQueryHelper)
    : IQueryHandler<SearchCodeRepoFilesQuery, IReadOnlyList<GitFileResult>>
{
    /// <inheritdoc />
    public async Task<ErrorOr<IReadOnlyList<GitFileResult>>> Handle(
        SearchCodeRepoFilesQuery query, CancellationToken cancellationToken)
    {
        var contextResult = await gitRepoQueryHelper.ResolveAsync(
            query.ProjectId, query.ConfigId, cancellationToken);
        if (contextResult.IsError)
            return contextResult.Errors;

        var ctx = contextResult.Value;
        return await ctx.Provider.SearchFilesAsync(
            ctx.Secret, ctx.Target.Owner, ctx.Target.RepositoryName,
            query.Branch, query.FilenamePattern, cancellationToken);
    }
}
