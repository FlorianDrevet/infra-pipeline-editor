using FluentAssertions;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Tests.InfrastructureConfigAggregate.ValueObjects;

public sealed class ConfigLayoutModeTests
{
    [Fact]
    public void Given_SameEnumValue_When_Compared_Then_AreEqual()
    {
        // Arrange
        var first = new ConfigLayoutMode(ConfigLayoutModeEnum.AllInOne);
        var second = new ConfigLayoutMode(ConfigLayoutModeEnum.AllInOne);

        // Act
        var equal = first.Equals(second);

        // Assert
        equal.Should().BeTrue();
        first.GetHashCode().Should().Be(second.GetHashCode());
    }

    [Fact]
    public void Given_DifferentEnumValues_When_Compared_Then_AreNotEqual()
    {
        // Arrange
        var first = new ConfigLayoutMode(ConfigLayoutModeEnum.AllInOne);
        var second = new ConfigLayoutMode(ConfigLayoutModeEnum.SplitInfraCode);

        // Act
        var equal = first.Equals(second);

        // Assert
        equal.Should().BeFalse();
    }

    [Fact]
    public void Given_EnumValue_When_Wrapped_Then_ExposesIt()
    {
        // Arrange
        const ConfigLayoutModeEnum expected = ConfigLayoutModeEnum.SplitInfraCode;

        // Act
        var sut = new ConfigLayoutMode(expected);

        // Assert
        sut.Value.Should().Be(expected);
    }
}
