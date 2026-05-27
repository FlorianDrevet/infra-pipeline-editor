using System.Reflection;
using System.Text.Json.Serialization;
using FluentAssertions;
using InfraFlowSculptor.BicepGeneration.Helpers;

namespace InfraFlowSculptor.BicepGeneration.Tests.Helpers;

public sealed class BicepObjectPropertyHelperTests
{
    private sealed record TestRecord(
        [property: JsonPropertyName("displayName")] string Name,
        [property: JsonPropertyName("itemCount")] int Count);

    private sealed record NoAttributeRecord(string FirstProp, int SecondProp);

    [Fact]
    public void Given_RecordWithJsonPropertyNames_When_EnumerateSerializedProperties_Then_UsesSerializedNames()
    {
        // Arrange
        var source = new TestRecord("hello", 42);

        // Act
        var properties = BicepObjectPropertyHelper.EnumerateSerializedProperties(source).ToList();

        // Assert
        properties.Should().HaveCount(2);
        properties.Should().Contain(p => p.PropertyName == "displayName" && (string)p.Value! == "hello");
        properties.Should().Contain(p => p.PropertyName == "itemCount" && (int)p.Value! == 42);
    }

    [Fact]
    public void Given_RecordWithoutJsonAttributes_When_EnumerateSerializedProperties_Then_UsesClrPropertyNames()
    {
        // Arrange
        var source = new NoAttributeRecord("value", 10);

        // Act
        var properties = BicepObjectPropertyHelper.EnumerateSerializedProperties(source).ToList();

        // Assert
        properties.Should().HaveCount(2);
        properties.Should().Contain(p => p.PropertyName == "FirstProp");
        properties.Should().Contain(p => p.PropertyName == "SecondProp");
    }

    [Fact]
    public void Given_NullSource_When_EnumerateSerializedProperties_Then_ThrowsArgumentNullException()
    {
        // Act
        var act = () => BicepObjectPropertyHelper.EnumerateSerializedProperties(null!).ToList();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Given_PropertyWithJsonPropertyNameAttribute_When_ResolveSerializedPropertyName_Then_ReturnsAttributeName()
    {
        // Arrange
        var property = typeof(TestRecord).GetProperty("Name")!;

        // Act
        var result = BicepObjectPropertyHelper.ResolveSerializedPropertyName(property);

        // Assert
        result.Should().Be("displayName");
    }

    [Fact]
    public void Given_PropertyWithoutAttribute_When_ResolveSerializedPropertyName_Then_ReturnsClrName()
    {
        // Arrange
        var property = typeof(NoAttributeRecord).GetProperty("FirstProp")!;

        // Act
        var result = BicepObjectPropertyHelper.ResolveSerializedPropertyName(property);

        // Assert
        result.Should().Be("FirstProp");
    }

    [Fact]
    public void Given_NullProperty_When_ResolveSerializedPropertyName_Then_ThrowsArgumentNullException()
    {
        // Act
        var act = () => BicepObjectPropertyHelper.ResolveSerializedPropertyName(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
