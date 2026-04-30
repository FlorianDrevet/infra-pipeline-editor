using FluentAssertions;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Tests.InfrastructureConfigAggregate.ValueObjects;

public sealed class InfrastructureConfigIdTests
{
    [Fact]
    public void Given_NoArgs_When_CreateUnique_Then_NewIdIsGenerated()
    {
        // Act
        var first = InfrastructureConfigId.CreateUnique();
        var second = InfrastructureConfigId.CreateUnique();

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
        var sut = InfrastructureConfigId.Create(expected);

        // Assert
        sut.Value.Should().Be(expected);
    }

    [Fact]
    public void Given_TwoInstancesWithSameGuid_When_Compared_Then_AreEqual()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var first = InfrastructureConfigId.Create(guid);
        var second = InfrastructureConfigId.Create(guid);

        // Act
        var equal = first.Equals(second);

        // Assert
        equal.Should().BeTrue();
        first.GetHashCode().Should().Be(second.GetHashCode());
    }
}
