using FluentAssertions;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Tests.InfrastructureConfigAggregate.ValueObjects;

public sealed class RoleTests
{
    [Fact]
    public void Given_SameEnumValue_When_Compared_Then_AreEqual()
    {
        // Arrange
        var first = new Role(Role.RoleEnum.Owner);
        var second = new Role(Role.RoleEnum.Owner);

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
        var first = new Role(Role.RoleEnum.Owner);
        var second = new Role(Role.RoleEnum.Reader);

        // Act
        var equal = first.Equals(second);

        // Assert
        equal.Should().BeFalse();
    }

    [Theory]
    [InlineData(Role.RoleEnum.Owner)]
    [InlineData(Role.RoleEnum.Contributor)]
    [InlineData(Role.RoleEnum.Reader)]
    public void Given_EnumValue_When_Wrapped_Then_ExposesIt(Role.RoleEnum value)
    {
        // Act
        var sut = new Role(value);

        // Assert
        sut.Value.Should().Be(value);
    }
}
