using FluentAssertions;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Tests.UserAggregate.ValueObjects;

public sealed class TagTests
{
    [Fact]
    public void Given_NameAndValue_When_Constructed_Then_ExposesBoth()
    {
        // Arrange
        const string name = "env";
        const string value = "prod";

        // Act
        var sut = new Tag(name, value);

        // Assert
        sut.Name.Should().Be(name);
        sut.Value.Should().Be(value);
    }

    [Fact]
    public void Given_TwoInstancesWithSameNameAndValue_When_Compared_Then_AreEqual()
    {
        // Arrange
        var first = new Tag("env", "prod");
        var second = new Tag("env", "prod");

        // Act
        var equal = first.Equals(second);

        // Assert
        equal.Should().BeTrue();
        first.GetHashCode().Should().Be(second.GetHashCode());
    }

    [Fact]
    public void Given_DifferentValues_When_Compared_Then_AreNotEqual()
    {
        // Arrange
        var first = new Tag("env", "prod");
        var second = new Tag("env", "dev");

        // Act
        var equal = first.Equals(second);

        // Assert
        equal.Should().BeFalse();
    }

    [Fact]
    public void Given_DifferentNames_When_Compared_Then_AreNotEqual()
    {
        // Arrange
        var first = new Tag("env", "prod");
        var second = new Tag("tier", "prod");

        // Act
        var equal = first.Equals(second);

        // Assert
        equal.Should().BeFalse();
    }
}
