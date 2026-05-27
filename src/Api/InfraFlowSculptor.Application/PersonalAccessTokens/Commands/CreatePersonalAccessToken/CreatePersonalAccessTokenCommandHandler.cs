using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.PersonalAccessTokens.Common;
using InfraFlowSculptor.Domain.PersonalAccessTokenAggregate;
using InfraFlowSculptor.Domain.PersonalAccessTokenAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.PersonalAccessTokens.Commands.CreatePersonalAccessToken;

/// <summary>
/// Handles the <see cref="CreatePersonalAccessTokenCommand"/> request,
/// generating a new PAT for the current user and persisting its hash.
/// </summary>
public sealed class CreatePersonalAccessTokenCommandHandler(
    IPersonalAccessTokenRepository repository,
    ICurrentUser currentUser)
    : ICommandHandler<CreatePersonalAccessTokenCommand, CreatedPersonalAccessTokenResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<CreatedPersonalAccessTokenResult>> Handle(
        CreatePersonalAccessTokenCommand request,
        CancellationToken cancellationToken)
    {
        var userId = await currentUser.GetUserIdAsync(cancellationToken);

        var scopes = ParseScopes(request.Scopes);

        var (token, plainTextToken) = PersonalAccessToken.Create(
            userId,
            request.Name,
            request.ExpiresAt,
            scopes);

        repository.Add(token);

        var result = new PersonalAccessTokenResult(
            token.Id,
            token.UserId,
            token.Name,
            token.TokenPrefix,
            token.ExpiresAt,
            token.CreatedAt,
            token.LastUsedAt,
            token.IsRevoked,
            token.Scopes.Select(s => s.Value.ToString()).ToList());

        return new CreatedPersonalAccessTokenResult(result, plainTextToken);
    }

    private static List<PatScope> ParseScopes(List<string>? scopeStrings)
    {
        if (scopeStrings is not { Count: > 0 })
        {
            return [new PatScope(PatScopeType.Read)];
        }

        return scopeStrings
            .Where(s => Enum.TryParse<PatScopeType>(s, ignoreCase: true, out _))
            .Select(s => new PatScope(Enum.Parse<PatScopeType>(s, ignoreCase: true)))
            .Distinct()
            .ToList();
    }
}
