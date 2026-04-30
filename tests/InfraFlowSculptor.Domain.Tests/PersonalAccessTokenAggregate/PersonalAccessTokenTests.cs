using FluentAssertions;
using InfraFlowSculptor.Domain.PersonalAccessTokenAggregate;
using InfraFlowSculptor.Domain.PersonalAccessTokenAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Tests.PersonalAccessTokenAggregate;

public sealed class PersonalAccessTokenTests
{
    private const string DefaultLabel = "ci-runner";
    private const string TokenPrefixLiteral = "ifs_";
    private static readonly DateTime FixedFutureExpiration = new(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime FixedPastExpiration = new(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime FixedNow = new(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);

    // ─── Create ─────────────────────────────────────────────────────────────

    [Fact]
    public void Given_UserAndName_When_Create_Then_ReturnsTokenAndPlainTextValue()
    {
        // Arrange
        var userId = UserId.CreateUnique();

        // Act
        var (token, plainText) = PersonalAccessToken.Create(userId, DefaultLabel, FixedFutureExpiration);

        // Assert
        token.Id.Should().NotBeNull();
        token.UserId.Should().Be(userId);
        token.Name.Should().Be(DefaultLabel);
        token.ExpiresAt.Should().Be(FixedFutureExpiration);
        token.IsRevoked.Should().BeFalse();
        token.LastUsedAt.Should().BeNull();
        token.CreatedAt.Should().NotBe(default);
        plainText.Should().StartWith(TokenPrefixLiteral);
        token.TokenPrefix.Should().StartWith(TokenPrefixLiteral);
        token.TokenPrefix.Length.Should().BeLessThanOrEqualTo(12);
        token.TokenHash.Value.Should().NotBe(plainText);
        token.TokenHash.Value.Should().HaveLength(64);
    }

    [Fact]
    public void Given_NoExpiration_When_Create_Then_ExpiresAtIsNull()
    {
        // Act
        var (token, _) = PersonalAccessToken.Create(UserId.CreateUnique(), DefaultLabel, expiresAt: null);

        // Assert
        token.ExpiresAt.Should().BeNull();
    }

    [Fact]
    public void Given_TwoCreateCalls_When_Compared_Then_PlainTextsAreDifferent()
    {
        // Arrange
        var userId = UserId.CreateUnique();

        // Act
        var (_, first) = PersonalAccessToken.Create(userId, DefaultLabel, FixedFutureExpiration);
        var (_, second) = PersonalAccessToken.Create(userId, DefaultLabel, FixedFutureExpiration);

        // Assert
        first.Should().NotBe(second);
    }

    [Fact]
    public void Given_PlainText_When_Create_Then_HashMatchesComputedHash()
    {
        // Act
        var (token, plainText) = PersonalAccessToken.Create(UserId.CreateUnique(), DefaultLabel, FixedFutureExpiration);

        // Assert
        var expected = TokenHash.Compute(plainText);
        token.TokenHash.Value.Should().Be(expected.Value);
    }

    // ─── Revoke ─────────────────────────────────────────────────────────────

    [Fact]
    public void Given_FreshToken_When_Revoke_Then_IsRevokedTrue()
    {
        // Arrange
        var (sut, _) = PersonalAccessToken.Create(UserId.CreateUnique(), DefaultLabel, FixedFutureExpiration);

        // Act
        sut.Revoke();

        // Assert
        sut.IsRevoked.Should().BeTrue();
    }

    // ─── RecordUsage ───────────────────────────────────────────────────────

    [Fact]
    public void Given_FreshToken_When_RecordUsage_Then_LastUsedAtIsSet()
    {
        // Arrange
        var (sut, _) = PersonalAccessToken.Create(UserId.CreateUnique(), DefaultLabel, FixedFutureExpiration);

        // Act
        sut.RecordUsage();

        // Assert
        sut.LastUsedAt.Should().NotBeNull();
    }

    // ─── IsValid ───────────────────────────────────────────────────────────

    [Fact]
    public void Given_FutureExpirationAndNotRevoked_When_IsValid_Then_ReturnsTrue()
    {
        // Arrange
        var (sut, _) = PersonalAccessToken.Create(UserId.CreateUnique(), DefaultLabel, FixedFutureExpiration);

        // Act
        var result = sut.IsValid(FixedNow);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Given_NoExpirationAndNotRevoked_When_IsValid_Then_ReturnsTrue()
    {
        // Arrange
        var (sut, _) = PersonalAccessToken.Create(UserId.CreateUnique(), DefaultLabel, expiresAt: null);

        // Act
        var result = sut.IsValid(FixedNow);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Given_PastExpiration_When_IsValid_Then_ReturnsFalse()
    {
        // Arrange
        var (sut, _) = PersonalAccessToken.Create(UserId.CreateUnique(), DefaultLabel, FixedPastExpiration);

        // Act
        var result = sut.IsValid(FixedNow);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Given_RevokedToken_When_IsValid_Then_ReturnsFalse()
    {
        // Arrange
        var (sut, _) = PersonalAccessToken.Create(UserId.CreateUnique(), DefaultLabel, FixedFutureExpiration);
        sut.Revoke();

        // Act
        var result = sut.IsValid(FixedNow);

        // Assert
        result.Should().BeFalse();
    }
}
