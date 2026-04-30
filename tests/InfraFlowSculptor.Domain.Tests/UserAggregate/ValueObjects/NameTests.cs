using FluentAssertions;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Tests.UserAggregate.ValueObjects;

public sealed class NameTests
{
    [Fact]
    public void Given_FirstAndLastName_When_Constructed_Then_ExposesBoth()
    {
        // Arrange
        const string firstName = "Ada";
        const string lastName = "Lovelace";

        // Act
        var sut = new Name(firstName, lastName);

        // Assert
        sut.FirstName.Should().Be(firstName);
        sut.LastName.Should().Be(lastName);
    }

    [Fact]
    public void Given_TwoInstancesWithSameValues_When_Compared_Then_AreEqual()
    {
        // Arrange
        var first = new Name("Ada", "Lovelace");
        var second = new Name("Ada", "Lovelace");

        // Act
        var equal = first.Equals(second);

        // Assert
        equal.Should().BeTrue();
        first.GetHashCode().Should().Be(second.GetHashCode());
    }

    [Fact]
    public void Given_DifferentFirstName_When_Compared_Then_AreNotEqual()
    {
        // Arrange
        var first = new Name("Ada", "Lovelace");
        var second = new Name("Grace", "Lovelace");

        // Act
        var equal = first.Equals(second);

        // Assert
        equal.Should().BeFalse();
    }

    [Fact]
    public void Given_DifferentLastName_When_Compared_Then_AreNotEqual()
    {
        // Arrange
        var first = new Name("Ada", "Lovelace");
        var second = new Name("Ada", "Hopper");

        // Act
        var equal = first.Equals(second);

        // Assert
        equal.Should().BeFalse();
    }
}
