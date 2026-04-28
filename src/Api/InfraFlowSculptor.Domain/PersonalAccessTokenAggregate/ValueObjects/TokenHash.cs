using System.Security.Cryptography;
using System.Text;
using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.PersonalAccessTokenAggregate.ValueObjects;

/// <summary>
/// SHA-256 hash of a personal access token's plaintext value.
/// The plaintext is never stored — only the hash is persisted.
/// </summary>
public sealed class TokenHash : SingleValueObject<string>
{
    /// <summary>EF Core constructor.</summary>
    public TokenHash() { }

    /// <summary>Initializes a new <see cref="TokenHash"/> with the given hash string.</summary>
    public TokenHash(string value) : base(value) { }

    /// <summary>
    /// Computes the SHA-256 hash of the given plaintext token.
    /// </summary>
    /// <param name="plainTextToken">The raw token value to hash.</param>
    /// <returns>A new <see cref="TokenHash"/> containing the lowercase hex digest.</returns>
    public static TokenHash Compute(string plainTextToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(plainTextToken));
        return new TokenHash(Convert.ToHexStringLower(bytes));
    }
}
