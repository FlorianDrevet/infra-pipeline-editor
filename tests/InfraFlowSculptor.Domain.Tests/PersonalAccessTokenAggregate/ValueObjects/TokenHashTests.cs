using FluentAssertions;
using InfraFlowSculptor.Domain.PersonalAccessTokenAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Tests.PersonalAccessTokenAggregate.ValueObjects;

public sealed class TokenHashTests
{
    private const string Sha256HexLength = "sha256 hex length";
    private const int ExpectedHashLength = 64;

    [Fact]
    public void Given_SamePlainText_When_Compute_Then_ProducesSameHash()
    {
        // Arrange
        const string plainText = "ifs_sample-token-value";

        // Act
        var first = TokenHash.Compute(plainText);
        var second = TokenHash.Compute(plainText);

        // Assert
        first.Value.Should().Be(second.Value);
    }

    [Fact]
    public void Given_DifferentPlainText_When_Compute_Then_ProducesDifferentHash()
    {
        // Act
        var first = TokenHash.Compute("ifs_first");
        var second = TokenHash.Compute("ifs_second");

        // Assert
        first.Value.Should().NotBe(second.Value);
    }

    [Fact]
    public void Given_PlainText_When_Compute_Then_ReturnsLowercaseHexOf64Chars()
    {
        // Act
        var sut = TokenHash.Compute("ifs_sample");

        // Assert
        sut.Value.Should().HaveLength(ExpectedHashLength, Sha256HexLength);
        sut.Value.Should().MatchRegex("^[0-9a-f]{64}$");
    }

    [Fact]
    public void Given_PlainText_When_Compute_Then_HashIsNotEqualToPlainText()
    {
        // Arrange
        const string plainText = "ifs_some-token-value";

        // Act
        var sut = TokenHash.Compute(plainText);

        // Assert
        sut.Value.Should().NotBe(plainText);
    }

    [Fact]
    public void Given_TwoHashesWithSameValue_When_Compared_Then_AreEqual()
    {
        // Arrange
        var first = new TokenHash("abcd1234");
        var second = new TokenHash("abcd1234");

        // Act
        var equal = first.Equals(second);

        // Assert
        equal.Should().BeTrue();
    }
}
