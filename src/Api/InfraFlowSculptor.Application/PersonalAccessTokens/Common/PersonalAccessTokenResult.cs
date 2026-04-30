using InfraFlowSculptor.Domain.PersonalAccessTokenAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.PersonalAccessTokens.Common;

/// <summary>
/// Read-only result representing a personal access token (without the plaintext value).
/// </summary>
/// <param name="Id">The token identifier.</param>
/// <param name="UserId">The owning user's identifier.</param>
/// <param name="Name">The human-readable label.</param>
/// <param name="TokenPrefix">The truncated prefix shown for identification.</param>
/// <param name="ExpiresAt">Optional UTC expiration date.</param>
/// <param name="CreatedAt">UTC creation timestamp.</param>
/// <param name="LastUsedAt">UTC timestamp of last authentication usage, or <c>null</c>.</param>
/// <param name="IsRevoked">Whether the token has been revoked.</param>
public record PersonalAccessTokenResult(
    PersonalAccessTokenId Id,
    UserId UserId,
    string Name,
    string TokenPrefix,
    DateTime? ExpiresAt,
    DateTime CreatedAt,
    DateTime? LastUsedAt,
    bool IsRevoked);
