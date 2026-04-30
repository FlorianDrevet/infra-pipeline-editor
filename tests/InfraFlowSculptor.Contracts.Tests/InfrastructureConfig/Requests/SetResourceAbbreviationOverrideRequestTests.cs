using FluentAssertions;
using InfraFlowSculptor.Contracts.InfrastructureConfig.Requests;
using InfraFlowSculptor.Contracts.Tests.TestSupport;

namespace InfraFlowSculptor.Contracts.Tests.InfrastructureConfig.Requests;

public sealed class SetResourceAbbreviationOverrideRequestTests
{
    [Fact]
    public void Given_LowercaseAlphanumericAbbreviation_When_Validate_Then_NoError()
    {
        // Arrange
        var sut = new SetResourceAbbreviationOverrideRequest
        {
            Abbreviation = "kv01",
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void Given_NullAbbreviation_When_Validate_Then_ReturnsRequiredError()
    {
        // Arrange
        var sut = new SetResourceAbbreviationOverrideRequest
        {
            Abbreviation = null!,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(SetResourceAbbreviationOverrideRequest.Abbreviation)).Should().BeTrue();
    }

    [Fact]
    public void Given_AbbreviationExceedingMaxLength_When_Validate_Then_ReturnsLengthError()
    {
        // Arrange
        var sut = new SetResourceAbbreviationOverrideRequest
        {
            Abbreviation = "abcdefghijk", // 11 chars, max 10
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(SetResourceAbbreviationOverrideRequest.Abbreviation)).Should().BeTrue();
    }

    [Theory]
    [InlineData("KV")]
    [InlineData("kv-01")]
    [InlineData("kv 01")]
    [InlineData("Kv1")]
    public void Given_NonLowercaseAlphanumeric_When_Validate_Then_ReturnsRegexError(string abbreviation)
    {
        // Arrange
        var sut = new SetResourceAbbreviationOverrideRequest
        {
            Abbreviation = abbreviation,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(SetResourceAbbreviationOverrideRequest.Abbreviation)).Should().BeTrue();
    }
}
