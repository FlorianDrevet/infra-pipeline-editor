using FluentAssertions;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Tests.ResourceGroupAggregate.ValueObjects;

public sealed class ResourceGroupIdTests
{
    [Fact]
    public void Given_NoArgs_When_CreateUnique_Then_NewIdIsGenerated()
    {
        // Act
        var first = ResourceGroupId.CreateUnique();
        var second = ResourceGroupId.CreateUnique();

        // Assert
        first.Value.Should().NotBe(Guid.Empty);
        first.Equals(second).Should().BeFalse();
    }

    [Fact]
    public void Given_SpecificGuid_When_Create_Then_WrapsValue()
    {
        // Arrange
        var expected = Guid.NewGuid();

        // Act
        var sut = ResourceGroupId.Create(expected);

        // Assert
        sut.Value.Should().Be(expected);
    }

    [Fact]
    public void Given_TwoInstancesWithSameGuid_When_Compared_Then_AreEqual()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var first = ResourceGroupId.Create(guid);
        var second = ResourceGroupId.Create(guid);

        // Assert
        first.Equals(second).Should().BeTrue();
        first.GetHashCode().Should().Be(second.GetHashCode());
    }
}
