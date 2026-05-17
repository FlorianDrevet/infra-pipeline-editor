using FluentAssertions;
using InfraFlowSculptor.Contracts.Common.Requests;
using InfraFlowSculptor.Contracts.Projects.Requests;
using InfraFlowSculptor.Contracts.Tests.TestSupport;

namespace InfraFlowSculptor.Contracts.Tests.Projects.Requests;

public sealed class SetProjectTagsRequestTests
{
    [Fact]
    public void Given_EmptyTags_When_Validate_Then_NoError()
    {
        // Arrange
        var sut = new SetProjectTagsRequest
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
        var sut = new SetProjectTagsRequest
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

        var sut = new SetProjectTagsRequest
        {
            Tags = tags,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(SetProjectTagsRequest.Tags)).Should().BeTrue();
    }
}
