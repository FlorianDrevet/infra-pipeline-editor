using FluentAssertions;
using InfraFlowSculptor.Domain.Common.ValueObjects;

namespace InfraFlowSculptor.Domain.Tests.Common.ValueObjects;

public sealed class NameTests
{
    [Fact]
    public void Given_NonEmptyValue_When_Constructed_Then_ExposesValue()
    {
        // Arrange
        const string value = "MyProject";

        // Act
        var sut = new Name(value);

        // Assert
        sut.Value.Should().Be(value);
    }

    [Fact]
    public void Given_TwoInstancesWithSameValue_When_Compared_Then_AreEqual()
    {
        // Arrange
        const string value = "Identical";
        var first = new Name(value);
        var second = new Name(value);

        // Act
        var equal = first.Equals(second);

        // Assert
        equal.Should().BeTrue();
        first.GetHashCode().Should().Be(second.GetHashCode());
    }

    [Fact]
    public void Given_TwoInstancesWithDifferentValues_When_Compared_Then_AreNotEqual()
    {
        // Arrange
        var first = new Name("Alpha");
        var second = new Name("Beta");

        // Act
        var equal = first.Equals(second);

        // Assert
        equal.Should().BeFalse();
    }

    [Fact]
    public void Given_Name_When_ImplicitlyConverted_Then_ReturnsUnderlyingString()
    {
        // Arrange
        const string value = "Implicit";
        var sut = new Name(value);

        // Act
        string converted = sut;

        // Assert
        converted.Should().Be(value);
    }
}
