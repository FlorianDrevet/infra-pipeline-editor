using FluentAssertions;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects.ResourceParameterUsage;

namespace InfraFlowSculptor.Domain.Tests.InfrastructureConfigAggregate.ValueObjects.ResourceParameterUsage;

public sealed class ResourceParameterUsageIdTests
{
    [Fact]
    public void Given_NoArgs_When_CreateUnique_Then_NewIdIsGenerated()
    {
        // Act
        var first = ResourceParameterUsageId.CreateUnique();
        var second = ResourceParameterUsageId.CreateUnique();

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
        var sut = ResourceParameterUsageId.Create(expected);

        // Assert
        sut.Value.Should().Be(expected);
    }
}
