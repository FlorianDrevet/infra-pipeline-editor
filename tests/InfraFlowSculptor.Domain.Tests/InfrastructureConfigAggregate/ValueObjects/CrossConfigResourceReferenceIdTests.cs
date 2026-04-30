using FluentAssertions;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Tests.InfrastructureConfigAggregate.ValueObjects;

public sealed class CrossConfigResourceReferenceIdTests
{
    [Fact]
    public void Given_NoArgs_When_CreateUnique_Then_NewIdIsGenerated()
    {
        // Act
        var first = CrossConfigResourceReferenceId.CreateUnique();
        var second = CrossConfigResourceReferenceId.CreateUnique();

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
        var sut = CrossConfigResourceReferenceId.Create(expected);

        // Assert
        sut.Value.Should().Be(expected);
    }
}
