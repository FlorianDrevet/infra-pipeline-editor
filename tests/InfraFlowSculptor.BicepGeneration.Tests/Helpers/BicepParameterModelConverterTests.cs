using System.Text.Json.Serialization;
using FluentAssertions;
using InfraFlowSculptor.BicepGeneration.Helpers;

namespace InfraFlowSculptor.BicepGeneration.Tests.Helpers;

public sealed class BicepParameterModelConverterTests
{
    private sealed record SimpleModel(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("count")] int Count,
        [property: JsonPropertyName("enabled")] bool Enabled);

    private sealed record ModelWithNullable(
        [property: JsonPropertyName("required")] string Required,
        [property: JsonPropertyName("optional")] string? Optional);

    private sealed record NestedModel(
        [property: JsonPropertyName("inner")] SimpleModel Inner);

    private sealed record ModelWithArray(
        [property: JsonPropertyName("tags")] string[] Tags);

    [Fact]
    public void Given_SimpleModel_When_ToDictionary_Then_ReturnsAllProperties()
    {
        // Arrange
        var model = new SimpleModel("test-vault", 3, true);

        // Act
        var result = BicepParameterModelConverter.ToDictionary(model);

        // Assert
        result.Should().ContainKey("name").WhoseValue.Should().Be("test-vault");
        result.Should().ContainKey("count").WhoseValue.Should().Be(3);
        result.Should().ContainKey("enabled").WhoseValue.Should().Be(true);
    }

    [Fact]
    public void Given_ModelWithNullProperty_When_ToDictionary_Then_OmitsNullProperty()
    {
        // Arrange
        var model = new ModelWithNullable("value", null);

        // Act
        var result = BicepParameterModelConverter.ToDictionary(model);

        // Assert
        result.Should().ContainKey("required");
        result.Should().NotContainKey("optional");
    }

    [Fact]
    public void Given_NestedModel_When_ToDictionary_Then_ReturnsNestedDictionary()
    {
        // Arrange
        var model = new NestedModel(new SimpleModel("inner-name", 1, false));

        // Act
        var result = BicepParameterModelConverter.ToDictionary(model);

        // Assert
        result.Should().ContainKey("inner");
        var inner = result["inner"].Should().BeAssignableTo<IReadOnlyDictionary<string, object>>().Subject;
        inner.Should().ContainKey("name").WhoseValue.Should().Be("inner-name");
        inner.Should().ContainKey("count").WhoseValue.Should().Be(1);
        inner.Should().ContainKey("enabled").WhoseValue.Should().Be(false);
    }

    [Fact]
    public void Given_ModelWithArray_When_ToDictionary_Then_ReturnsListValue()
    {
        // Arrange
        var model = new ModelWithArray(["tag1", "tag2"]);

        // Act
        var result = BicepParameterModelConverter.ToDictionary(model);

        // Assert
        result.Should().ContainKey("tags");
        var tags = result["tags"].Should().BeAssignableTo<List<object>>().Subject;
        tags.Should().HaveCount(2);
        tags[0].Should().Be("tag1");
        tags[1].Should().Be("tag2");
    }

    [Fact]
    public void Given_NullModel_When_ToDictionary_Then_ThrowsArgumentNullException()
    {
        // Act
        var act = () => BicepParameterModelConverter.ToDictionary<SimpleModel>(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Given_SimpleModel_When_ToValue_Then_ReturnsDictionaryObject()
    {
        // Arrange
        var model = new SimpleModel("test", 5, true);

        // Act
        var result = BicepParameterModelConverter.ToValue(model);

        // Assert
        result.Should().BeAssignableTo<Dictionary<string, object>>();
        var dict = (Dictionary<string, object>)result;
        dict.Should().ContainKey("name").WhoseValue.Should().Be("test");
    }

    [Fact]
    public void Given_NullModel_When_ToValue_Then_ThrowsArgumentNullException()
    {
        // Act
        var act = () => BicepParameterModelConverter.ToValue<SimpleModel>(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
