using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.PersonalAccessTokenAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.PersonalAccessTokenAggregate;

/// <summary>
/// Represents a personal access token (PAT) that authenticates a user
/// for programmatic access (e.g. MCP clients, CI/CD integrations).
/// The plaintext token is returned only at creation time; only the hash is persisted.
/// </summary>
public sealed class PersonalAccessToken : AggregateRoot<PersonalAccessTokenId>
{
    /// <summary>Prefix prepended to every generated token for quick identification.</summary>
    private const string TokenPrefix = "ifs_";

    /// <summary>Number of random bytes used to generate the token entropy.</summary>
    private const int TokenEntropyBytes = 32;

    /// <summary>Gets the identifier of the user who owns this token.</summary>
    public required UserId UserId { get; init; }

    /// <summary>Gets the human-readable label assigned to this token.</summary>
    public required string Name { get; init; }

    /// <summary>Gets the SHA-256 hash of the plaintext token.</summary>
    public required TokenHash TokenHash { get; init; }

    /// <summary>Gets the truncated prefix of the token shown for identification (e.g. "ifs_abc1").</summary>
    public required string TokenPrefix_ { get; init; }

    /// <summary>Gets the optional UTC expiration date. <c>null</c> means the token never expires.</summary>
    public DateTime? ExpiresAt { get; private set; }

    /// <summary>Gets the UTC date when the token was created.</summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>Gets the UTC date when the token was last used for authentication, or <c>null</c> if never used.</summary>
    public DateTime? LastUsedAt { get; private set; }

    /// <summary>Gets whether this token has been revoked.</summary>
    public bool IsRevoked { get; private set; }

    [SetsRequiredMembers]
    private PersonalAccessToken(
        PersonalAccessTokenId id,
        UserId userId,
        string name,
        TokenHash tokenHash,
        string tokenPrefix,
        DateTime? expiresAt,
        DateTime createdAt)
        : base(id)
    {
        UserId = userId;
        Name = name;
        TokenHash = tokenHash;
        TokenPrefix_ = tokenPrefix;
        ExpiresAt = expiresAt;
        CreatedAt = createdAt;
        IsRevoked = false;
    }

    /// <summary>
    /// Creates a new personal access token and returns both the persisted entity and the plaintext token.
    /// The plaintext token is only available at creation time and must be shown to the user immediately.
    /// </summary>
    /// <param name="userId">The identifier of the owning user.</param>
    /// <param name="name">A human-readable label for this token.</param>
    /// <param name="expiresAt">Optional UTC expiration date.</param>
    /// <returns>A tuple of the persisted entity and the one-time plaintext token value.</returns>
    public static (PersonalAccessToken Token, string PlainTextToken) Create(
        UserId userId,
        string name,
        DateTime? expiresAt)
    {
        var plainText = GenerateToken();
        var hash = ValueObjects.TokenHash.Compute(plainText);
        var prefix = plainText[..Math.Min(12, plainText.Length)];

        var token = new PersonalAccessToken(
            PersonalAccessTokenId.CreateUnique(),
            userId,
            name,
            hash,
            prefix,
            expiresAt,
            DateTime.UtcNow);

        return (token, plainText);
    }

    /// <summary>Marks this token as revoked. Revoked tokens cannot be used for authentication.</summary>
    public void Revoke()
    {
        IsRevoked = true;
    }

    /// <summary>Records the current UTC time as the last usage timestamp.</summary>
    public void RecordUsage()
    {
        LastUsedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Returns <c>true</c> if this token is valid for authentication
    /// (not revoked and not expired relative to <paramref name="utcNow"/>).
    /// </summary>
    /// <param name="utcNow">The current UTC time to check expiration against.</param>
    public bool IsValid(DateTime utcNow)
    {
        if (IsRevoked) return false;
        if (ExpiresAt.HasValue && ExpiresAt.Value < utcNow) return false;
        return true;
    }

    /// <summary>EF Core constructor.</summary>
    public PersonalAccessToken() { }

    private static string GenerateToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(TokenEntropyBytes);
        var encoded = Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
        return $"{TokenPrefix}{encoded}";
    }
}
