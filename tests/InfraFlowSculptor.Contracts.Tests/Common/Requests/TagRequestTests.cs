using FluentAssertions;
using InfraFlowSculptor.Contracts.Common.Requests;
using InfraFlowSculptor.Contracts.Tests.TestSupport;

namespace InfraFlowSculptor.Contracts.Tests.Common.Requests;

public sealed class TagRequestTests
{
    [Fact]
    public void Given_ValidNameAndValue_When_Validate_Then_NoError()
    {
        // Arrange
        var sut = new TagRequest
        {
            Name = "environment",
            Value = "production",
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void Given_NullName_When_Validate_Then_ReturnsRequiredError()
    {
        // Arrange
        var sut = new TagRequest
        {
            Name = null!,
            Value = "production",
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.Should().ContainSingle()
            .Which.MemberNames.Should().Contain(nameof(TagRequest.Name));
    }

    [Fact]
    public void Given_NullValue_When_Validate_Then_ReturnsRequiredError()
    {
        // Arrange
        var sut = new TagRequest
        {
            Name = "environment",
            Value = null!,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.Should().ContainSingle()
            .Which.MemberNames.Should().Contain(nameof(TagRequest.Value));
    }

    [Fact]
    public void Given_NameExceedingMaxLength_When_Validate_Then_ReturnsLengthError()
    {
        // Arrange
        var sut = new TagRequest
        {
            Name = new string('n', 513),
            Value = "ok",
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.Should().ContainSingle()
            .Which.MemberNames.Should().Contain(nameof(TagRequest.Name));
    }

    [Fact]
    public void Given_ValueExceedingMaxLength_When_Validate_Then_ReturnsLengthError()
    {
        // Arrange
        var sut = new TagRequest
        {
            Name = "environment",
            Value = new string('v', 257),
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.Should().ContainSingle()
            .Which.MemberNames.Should().Contain(nameof(TagRequest.Value));
    }
}
