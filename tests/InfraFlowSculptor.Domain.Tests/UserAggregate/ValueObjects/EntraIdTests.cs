using FluentAssertions;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Tests.UserAggregate.ValueObjects;

public sealed class EntraIdTests
{
    [Fact]
    public void Given_ValidGuid_When_Constructed_Then_ExposesValue()
    {
        // Arrange
        var value = Guid.NewGuid();

        // Act
        var sut = new EntraId(value);

        // Assert
        sut.Value.Should().Be(value);
    }

    [Fact]
    public void Given_EmptyGuid_When_Constructed_Then_ThrowsArgumentException()
    {
        // Arrange
        Action act = () => _ = new EntraId(Guid.Empty);

        // Act
        var assertion = act.Should().Throw<ArgumentException>();

        // Assert
        assertion.Which.ParamName.Should().Be("value");
    }
}