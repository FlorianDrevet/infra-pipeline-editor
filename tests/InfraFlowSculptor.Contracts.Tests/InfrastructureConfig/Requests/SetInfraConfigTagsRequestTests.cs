using FluentAssertions;
using InfraFlowSculptor.Contracts.Common.Requests;
using InfraFlowSculptor.Contracts.InfrastructureConfig.Requests;
using InfraFlowSculptor.Contracts.Tests.TestSupport;

namespace InfraFlowSculptor.Contracts.Tests.InfrastructureConfig.Requests;

public sealed class SetInfraConfigTagsRequestTests
{
    [Fact]
    public void Given_EmptyTags_When_Validate_Then_NoError()
    {
        // Arrange
        var sut = new SetInfraConfigTagsRequest
        {
            Tags = [],
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void Given_ValidTags_When_Validate_Then_NoError()
    {
        // Arrange
        var sut = new SetInfraConfigTagsRequest
        {
            Tags =
            [
                new TagRequest { Name = "env", Value = "prod" },
                new TagRequest { Name = "team", Value = "backend" },
            ],
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void Given_TooManyTags_When_Validate_Then_ReturnsError()
    {
        // Arrange
        var tags = Enumerable.Range(0, TagRequestConstraints.MaxTagCount + 1)
            .Select(i => new TagRequest { Name = $"tag-{i}", Value = $"value-{i}" })
            .ToList();

        var sut = new SetInfraConfigTagsRequest
        {
            Tags = tags,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(SetInfraConfigTagsRequest.Tags)).Should().BeTrue();
    }
}
