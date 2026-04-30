using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;

namespace InfraFlowSculptor.Application.PersonalAccessTokens.Commands.RevokePersonalAccessToken;

/// <summary>
/// Handles the <see cref="RevokePersonalAccessTokenCommand"/> request,
/// revoking the token only if it belongs to the current user.
/// </summary>
public sealed class RevokePersonalAccessTokenCommandHandler(
    IPersonalAccessTokenRepository repository,
    ICurrentUser currentUser)
    : ICommandHandler<RevokePersonalAccessTokenCommand, bool>
{
    /// <inheritdoc />
    public async Task<ErrorOr<bool>> Handle(
        RevokePersonalAccessTokenCommand request,
        CancellationToken cancellationToken)
    {
        var userId = await currentUser.GetUserIdAsync(cancellationToken);

        var token = await repository.GetByIdAsync(request.Id, cancellationToken);

        if (token is null || token.UserId != userId)
            return Errors.PersonalAccessToken.NotFound(request.Id);

        if (token.IsRevoked)
            return Errors.PersonalAccessToken.AlreadyRevoked(request.Id);

        token.Revoke();

        return true;
    }
}
