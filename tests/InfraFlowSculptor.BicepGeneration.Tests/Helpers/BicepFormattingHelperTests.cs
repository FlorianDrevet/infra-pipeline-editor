using FluentAssertions;
using InfraFlowSculptor.BicepGeneration.Helpers;
using System.Text.Json.Serialization;

namespace InfraFlowSculptor.BicepGeneration.Tests.Helpers;

public sealed class BicepFormattingHelperTests
{
    [Theory]
    [InlineData("storageAccount")]
    [InlineData("_sharedType")]
    [InlineData("key1")]
    public void Given_ValidBicepObjectKey_When_Formatting_Then_ReturnsUnquotedIdentifier(string key)
    {
        // Act
        var result = BicepFormattingHelper.FormatBicepObjectKey(key);

        // Assert
        result.Should().Be(key);
    }

    [Theory]
    [InlineData("storage-account", "'storage-account'")]
    [InlineData("storage account", "'storage account'")]
    [InlineData("o'clock", "'o\\'clock'")]
    public void Given_InvalidBicepObjectKey_When_Formatting_Then_ReturnsQuotedEscapedValue(string key, string expected)
    {
        // Act
        var result = BicepFormattingHelper.FormatBicepObjectKey(key);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void Given_ObjectWithJsonPropertyNames_When_Serializing_Then_UsesAnnotatedNamesAndSkipsNulls()
    {
        // Arrange
        var value = new AnnotatedParameterObject
        {
            RuntimeStack = "DOTNETCORE",
            OptionalSetting = null,
            Nested = new NestedAnnotatedParameterObject
            {
                TargetPort = 8080,
            },
        };

        // Act
        var result = BicepFormattingHelper.SerializeToBicep(value);

        // Assert
        result.Should().Contain("runtimeStack: 'DOTNETCORE'");
        result.Should().Contain("nested: {");
        result.Should().Contain("targetPort: 8080");
        result.Should().NotContain("RuntimeStack");
        result.Should().NotContain("Nested");
        result.Should().NotContain("OptionalSetting");
        result.Should().NotContain("optionalSetting");
    }

    [Fact]
    public void Given_InvalidAnnotatedAndDictionaryKeys_When_Serializing_Then_FormatsBicepObjectKeys()
    {
        // Arrange
        var value = new InvalidKeyAnnotatedParameterObject
        {
            RuntimeStack = "DOTNETCORE",
            EnvironmentVariables = new Dictionary<string, object>
            {
                ["startup command"] = "run",
            },
        };

        // Act
        var result = BicepFormattingHelper.SerializeToBicep(value);

        // Assert
        result.Should().Contain("'runtime-stack': 'DOTNETCORE'");
        result.Should().Contain("'environment-variables': {");
        result.Should().Contain("'startup command': 'run'");
    }

    private sealed class AnnotatedParameterObject
    {
        [JsonPropertyName("runtimeStack")]
        public string RuntimeStack { get; init; } = string.Empty;

        [JsonPropertyName("optionalSetting")]
        public string? OptionalSetting { get; init; }

        [JsonPropertyName("nested")]
        public NestedAnnotatedParameterObject Nested { get; init; } = new();
    }

    private sealed class NestedAnnotatedParameterObject
    {
        [JsonPropertyName("targetPort")]
        public int TargetPort { get; init; }
    }

    private sealed class InvalidKeyAnnotatedParameterObject
    {
        [JsonPropertyName("runtime-stack")]
        public string RuntimeStack { get; init; } = string.Empty;

        [JsonPropertyName("environment-variables")]
        public Dictionary<string, object> EnvironmentVariables { get; init; } = [];
    }
}