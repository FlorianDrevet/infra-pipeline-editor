using FluentAssertions;
using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.Tests.Common.Models;

/// <summary>
/// Tests for <see cref="SingleValueObject{T}"/> base behavior, in particular the
/// <see cref="object.ToString"/> override added under audit DDD-012.
/// </summary>
public sealed class SingleValueObjectTests
{
    private sealed class StringWrapper(string value) : SingleValueObject<string>(value);

    private sealed class IntWrapper(int value) : SingleValueObject<int>(value);

    private sealed class GuidWrapper(Guid value) : SingleValueObject<Guid>(value);

    private sealed class NullableStringWrapper : SingleValueObject<string?>
    {
        public NullableStringWrapper(string? value) : base(value) { }
    }

    [Fact]
    public void Given_StringWrappedValue_When_ToString_Then_ReturnsUnderlyingValue()
    {
        // Arrange
        var sut = new StringWrapper("payload");

        // Act
        var rendered = sut.ToString();

        // Assert
        rendered.Should().Be("payload");
    }

    [Fact]
    public void Given_IntWrappedValue_When_ToString_Then_ReturnsInvariantStringRepresentation()
    {
        // Arrange
        var sut = new IntWrapper(42);

        // Act
        var rendered = sut.ToString();

        // Assert
        rendered.Should().Be("42");
    }

    [Fact]
    public void Given_GuidWrappedValue_When_ToString_Then_ReturnsGuidString()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var sut = new GuidWrapper(guid);

        // Act
        var rendered = sut.ToString();

        // Assert
        rendered.Should().Be(guid.ToString());
    }

    [Fact]
    public void Given_NullUnderlyingValue_When_ToString_Then_ReturnsEmptyString()
    {
        // Arrange
        var sut = new NullableStringWrapper(null);

        // Act
        var rendered = sut.ToString();

        // Assert
        rendered.Should().BeEmpty();
    }
}
