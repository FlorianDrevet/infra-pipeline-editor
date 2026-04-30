using FluentAssertions;
using InfraFlowSculptor.Domain.Common.ValueObjects;

namespace InfraFlowSculptor.Domain.Tests.Common.ValueObjects;

public sealed class OrderTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(42)]
    public void Given_NonNegativeValue_When_Constructed_Then_ExposesValue(int value)
    {
        // Act
        var sut = new Order(value);

        // Assert
        sut.Value.Should().Be(value);
    }

    [Fact]
    public void Given_TwoInstancesWithSameValue_When_Compared_Then_AreEqual()
    {
        // Arrange
        var first = new Order(3);
        var second = new Order(3);

        // Act
        var equal = first.Equals(second);

        // Assert
        equal.Should().BeTrue();
    }

    [Fact]
    public void Given_TwoInstancesWithDifferentValues_When_Compared_Then_AreNotEqual()
    {
        // Arrange
        var first = new Order(1);
        var second = new Order(2);

        // Act
        var equal = first.Equals(second);

        // Assert
        equal.Should().BeFalse();
    }
}
