using FluentAssertions;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects.ParameterDefinition;

namespace InfraFlowSculptor.Domain.Tests.InfrastructureConfigAggregate.ValueObjects.ParameterDefinition;

public sealed class IsSecretTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Given_BooleanValue_When_Constructed_Then_ExposesValue(bool value)
    {
        // Act
        var sut = new IsSecret(value);

        // Assert
        sut.Value.Should().Be(value);
    }

    [Fact]
    public void Given_TwoInstancesWithSameValue_When_Compared_Then_AreEqual()
    {
        // Arrange
        var first = new IsSecret(true);
        var second = new IsSecret(true);

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
        var first = new IsSecret(true);
        var second = new IsSecret(false);

        // Act
        var equal = first.Equals(second);

        // Assert
        equal.Should().BeFalse();
    }
}
