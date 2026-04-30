using ErrorOr;
using InfraFlowSculptor.Domain.PersonalAccessTokenAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Common.Errors;

/// <summary>Partial error definitions for domain aggregates.</summary>
public static partial class Errors
{
    /// <summary>Domain errors for the <see cref="PersonalAccessTokenAggregate.PersonalAccessToken"/> aggregate.</summary>
    public static class PersonalAccessToken
    {
        private const string NotFoundCode = "PersonalAccessToken.NotFound";
        private const string AlreadyRevokedCode = "PersonalAccessToken.AlreadyRevoked";

        /// <summary>Returns a not-found error for the given identifier.</summary>
        public static Error NotFound(PersonalAccessTokenId id) =>
            Error.NotFound(code: NotFoundCode, description: $"No personal access token with id {id} was found.");

        /// <summary>Returns a conflict error when the token is already revoked.</summary>
        public static Error AlreadyRevoked(PersonalAccessTokenId id) =>
            Error.Conflict(code: AlreadyRevokedCode, description: $"Personal access token {id} is already revoked.");
    }
}
