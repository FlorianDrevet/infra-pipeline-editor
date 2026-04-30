using FluentAssertions;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Tests.ProjectAggregate.ValueObjects;

public sealed class LayoutPresetTests
{
    [Theory]
    [InlineData(LayoutPresetEnum.AllInOne)]
    [InlineData(LayoutPresetEnum.SplitInfraCode)]
    [InlineData(LayoutPresetEnum.MultiRepo)]
    public void Given_Enum_When_Constructed_Then_ExposesValue(LayoutPresetEnum value)
    {
        // Act
        var sut = new LayoutPreset(value);

        // Assert
        sut.Value.Should().Be(value);
    }

    [Fact]
    public void Given_TwoInstancesWithSameEnum_When_Compared_Then_AreEqual()
    {
        // Arrange
        var first = new LayoutPreset(LayoutPresetEnum.MultiRepo);
        var second = new LayoutPreset(LayoutPresetEnum.MultiRepo);

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
        var first = new LayoutPreset(LayoutPresetEnum.AllInOne);
        var second = new LayoutPreset(LayoutPresetEnum.SplitInfraCode);

        // Act
        var equal = first.Equals(second);

        // Assert
        equal.Should().BeFalse();
    }
}
