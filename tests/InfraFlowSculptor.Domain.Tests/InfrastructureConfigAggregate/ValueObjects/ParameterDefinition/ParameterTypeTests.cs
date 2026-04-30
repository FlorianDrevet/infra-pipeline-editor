using FluentAssertions;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects.ParameterDefinition;

namespace InfraFlowSculptor.Domain.Tests.InfrastructureConfigAggregate.ValueObjects.ParameterDefinition;

public sealed class ParameterTypeTests
{
    [Theory]
    [InlineData(ParameterType.Enum.String)]
    [InlineData(ParameterType.Enum.Int)]
    [InlineData(ParameterType.Enum.Bool)]
    [InlineData(ParameterType.Enum.Object)]
    [InlineData(ParameterType.Enum.Array)]
    public void Given_EnumValue_When_Wrapped_Then_ExposesValue(ParameterType.Enum value)
    {
        // Act
        var sut = new ParameterType(value);

        // Assert
        sut.Value.Should().Be(value);
    }

    [Fact]
    public void Given_TwoInstancesWithSameEnumValue_When_Compared_Then_AreEqual()
    {
        // Arrange
        var first = new ParameterType(ParameterType.Enum.String);
        var second = new ParameterType(ParameterType.Enum.String);

        // Act
        var equal = first.Equals(second);

        // Assert
        equal.Should().BeTrue();
        first.GetHashCode().Should().Be(second.GetHashCode());
    }

    [Fact]
    public void Given_TwoInstancesWithDifferentEnumValues_When_Compared_Then_AreNotEqual()
    {
        // Arrange
        var first = new ParameterType(ParameterType.Enum.String);
        var second = new ParameterType(ParameterType.Enum.Int);

        // Act
        var equal = first.Equals(second);

        // Assert
        equal.Should().BeFalse();
    }
}
