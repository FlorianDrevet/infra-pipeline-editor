using FluentAssertions;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Tests.ProjectAggregate.ValueObjects;

public sealed class GitProviderTypeTests
{
    [Theory]
    [InlineData(GitProviderTypeEnum.GitHub)]
    [InlineData(GitProviderTypeEnum.AzureDevOps)]
    public void Given_Enum_When_Constructed_Then_ExposesValue(GitProviderTypeEnum value)
    {
        // Act
        var sut = new GitProviderType(value);

        // Assert
        sut.Value.Should().Be(value);
    }

    [Fact]
    public void Given_TwoInstancesWithSameEnum_When_Compared_Then_AreEqual()
    {
        // Arrange
        var first = new GitProviderType(GitProviderTypeEnum.GitHub);
        var second = new GitProviderType(GitProviderTypeEnum.GitHub);

        // Act
        var equal = first.Equals(second);

        // Assert
        equal.Should().BeTrue();
        first.GetHashCode().Should().Be(second.GetHashCode());
    }

    [Fact]
    public void Given_TwoInstancesWithDifferentEnums_When_Compared_Then_AreNotEqual()
    {
        // Arrange
        var first = new GitProviderType(GitProviderTypeEnum.GitHub);
        var second = new GitProviderType(GitProviderTypeEnum.AzureDevOps);

        // Act
        var equal = first.Equals(second);

        // Assert
        equal.Should().BeFalse();
    }
}
