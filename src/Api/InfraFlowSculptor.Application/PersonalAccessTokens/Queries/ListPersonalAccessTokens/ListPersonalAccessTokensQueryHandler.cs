using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.PersonalAccessTokens.Common;

namespace InfraFlowSculptor.Application.PersonalAccessTokens.Queries.ListPersonalAccessTokens;

/// <summary>
/// Handles the <see cref="ListPersonalAccessTokensQuery"/> request,
/// returning all tokens owned by the current user.
/// </summary>
public sealed class ListPersonalAccessTokensQueryHandler(
    IPersonalAccessTokenRepository repository,
    ICurrentUser currentUser)
    : IQueryHandler<ListPersonalAccessTokensQuery, List<PersonalAccessTokenResult>>
{
    /// <inheritdoc />
    public async Task<ErrorOr<List<PersonalAccessTokenResult>>> Handle(
        ListPersonalAccessTokensQuery request,
        CancellationToken cancellationToken)
    {
        var userId = await currentUser.GetUserIdAsync(cancellationToken);

        var tokens = await repository.GetByUserIdAsync(userId, cancellationToken);

        var results = tokens
            .Select(t => new PersonalAccessTokenResult(
                t.Id,
                t.UserId,
                t.Name,
                t.TokenPrefix,
                t.ExpiresAt,
                t.CreatedAt,
                t.LastUsedAt,
                t.IsRevoked))
            .ToList();

        return results;
    }
}
