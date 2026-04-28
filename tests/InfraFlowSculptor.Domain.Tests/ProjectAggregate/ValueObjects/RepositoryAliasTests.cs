using FluentAssertions;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Tests.ProjectAggregate.ValueObjects;

public sealed class RepositoryAliasTests
{
    [Fact]
    public void Given_ValidLowercaseSlug_When_Create_Then_ReturnsRepositoryAlias()
    {
        // Arrange
        const string value = "infra-repo-01";

        // Act
        var result = RepositoryAlias.Create(value);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Value.Should().Be(value);
    }

    [Theory]
    [InlineData("InfraRepo")]
    [InlineData("infra_repo")]
    public void Given_InvalidSlugCharacters_When_Create_Then_ReturnsValidationError(string value)
    {
        // Act
        var result = RepositoryAlias.Create(value);

        // Assert
        result.IsError.Should().BeTrue();
    }
}