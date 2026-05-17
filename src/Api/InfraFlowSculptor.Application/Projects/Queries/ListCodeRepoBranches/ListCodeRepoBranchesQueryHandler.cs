using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Projects.Common;

namespace InfraFlowSculptor.Application.Projects.Queries.ListCodeRepoBranches;

/// <summary>Handles the <see cref="ListCodeRepoBranchesQuery"/>.</summary>
public sealed class ListCodeRepoBranchesQueryHandler(IGitRepoQueryHelper gitRepoQueryHelper)
    : IQueryHandler<ListCodeRepoBranchesQuery, IReadOnlyList<GitBranchResult>>
{
    /// <inheritdoc />
    public async Task<ErrorOr<IReadOnlyList<GitBranchResult>>> Handle(
        ListCodeRepoBranchesQuery query, CancellationToken cancellationToken)
    {
        var contextResult = await gitRepoQueryHelper.ResolveAsync(
            query.ProjectId, query.ConfigId, cancellationToken);
        if (contextResult.IsError)
            return contextResult.Errors;

        var ctx = contextResult.Value;
        return await ctx.Provider.ListBranchesAsync(
            ctx.Secret, ctx.Target.Owner, ctx.Target.RepositoryName, cancellationToken);
    }
}
