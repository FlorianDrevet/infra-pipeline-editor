using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.PersonalAccessTokens.Common;
using InfraFlowSculptor.Domain.PersonalAccessTokenAggregate;

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

        var (token, plainTextToken) = PersonalAccessToken.Create(
            userId,
            request.Name,
            request.ExpiresAt);

        await repository.AddAsync(token);

        var result = new PersonalAccessTokenResult(
            token.Id,
            token.UserId,
            token.Name,
            token.TokenPrefix_,
            token.ExpiresAt,
            token.CreatedAt,
            token.LastUsedAt,
            token.IsRevoked);

        return new CreatedPersonalAccessTokenResult(result, plainTextToken);
    }
}
