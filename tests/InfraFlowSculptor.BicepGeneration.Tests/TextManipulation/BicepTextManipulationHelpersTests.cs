using FluentAssertions;
using InfraFlowSculptor.BicepGeneration.TextManipulation;

namespace InfraFlowSculptor.BicepGeneration.Tests.TextManipulation;

public sealed class BicepTextManipulationHelpersTests
{
    // ── HasParam ──

    [Fact]
    public void Given_BicepWithMatchingParam_When_HasParam_Then_ReturnsTrue()
    {
        // Arrange
        const string bicep = "param location string\nparam tags object = {}\nresource kv 'Microsoft.KeyVault/vaults' = {";

        // Act
        var result = BicepTextManipulationHelpers.HasParam(bicep, "tags");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Given_BicepWithoutMatchingParam_When_HasParam_Then_ReturnsFalse()
    {
        // Arrange
        const string bicep = "param location string\nresource kv 'Microsoft.KeyVault/vaults' = {";

        // Act
        var result = BicepTextManipulationHelpers.HasParam(bicep, "tags");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Given_ParamNameSubstring_When_HasParam_Then_ReturnsFalse()
    {
        // Arrange
        const string bicep = "param tagsExtended object = {}\n";

        // Act — "tags" should not match "tagsExtended" thanks to \b boundary
        var result = BicepTextManipulationHelpers.HasParam(bicep, "tags");

        // Assert
        result.Should().BeFalse();
    }

    // ── HasOutput ──

    [Fact]
    public void Given_BicepWithMatchingOutput_When_HasOutput_Then_ReturnsTrue()
    {
        // Arrange
        const string bicep = "resource kv 'Microsoft.KeyVault/vaults' = {}\n\noutput vaultUri string = kv.properties.vaultUri\n";

        // Act
        var result = BicepTextManipulationHelpers.HasOutput(bicep, "vaultUri");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Given_BicepWithoutOutput_When_HasOutput_Then_ReturnsFalse()
    {
        // Arrange
        const string bicep = "resource kv 'Microsoft.KeyVault/vaults' = {}\n";

        // Act
        var result = BicepTextManipulationHelpers.HasOutput(bicep, "vaultUri");

        // Assert
        result.Should().BeFalse();
    }

    // ── FindFirstLineIndex ──

    [Fact]
    public void Given_ContentWithMatchingLine_When_FindFirstLineIndex_Then_ReturnsCorrectIndex()
    {
        // Arrange
        const string content = "param location string\nvar namePrefix = 'abc'\nresource kv 'foo' = {\n}";

        // Act
        var result = BicepTextManipulationHelpers.FindFirstLineIndex(content, "var ");

        // Assert
        result.Should().Be(content.IndexOf("var ", StringComparison.Ordinal));
    }

    [Fact]
    public void Given_ContentWithIndentedLine_When_FindFirstLineIndex_Then_ReturnsIndexIncludingIndent()
    {
        // Arrange
        const string content = "resource kv 'foo' = {\n  properties: {\n  }\n}";

        // Act
        var result = BicepTextManipulationHelpers.FindFirstLineIndex(content, "properties:");

        // Assert
        result.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Given_ContentWithoutMatch_When_FindFirstLineIndex_Then_ReturnsMinusOne()
    {
        // Arrange
        const string content = "param location string\nresource kv 'foo' = {}";

        // Act
        var result = BicepTextManipulationHelpers.FindFirstLineIndex(content, "var ");

        // Assert
        result.Should().Be(-1);
    }

    // ── InsertAt ──

    [Fact]
    public void Given_SourceAndContent_When_InsertAtMiddle_Then_InsertsCorrectly()
    {
        // Arrange
        const string source = "ABCDEF";
        const string insertion = "XYZ";
        const int position = 3;

        // Act
        var result = BicepTextManipulationHelpers.InsertAt(source, position, insertion);

        // Assert
        result.Should().Be("ABCXYZDEF");
    }

    [Fact]
    public void Given_Source_When_InsertAtBeginning_Then_PrependsContent()
    {
        // Arrange
        const string source = "hello";
        const string insertion = "world ";

        // Act
        var result = BicepTextManipulationHelpers.InsertAt(source, 0, insertion);

        // Assert
        result.Should().Be("world hello");
    }

    [Fact]
    public void Given_Source_When_InsertAtEnd_Then_AppendsContent()
    {
        // Arrange
        const string source = "hello";
        const string insertion = " world";

        // Act
        var result = BicepTextManipulationHelpers.InsertAt(source, source.Length, insertion);

        // Assert
        result.Should().Be("hello world");
    }

    // ── FindClosingBrace ──

    [Fact]
    public void Given_NestedBraces_When_FindClosingBrace_Then_ReturnsMatchingPosition()
    {
        // Arrange
        const string source = "resource kv 'foo' = {\n  properties: {\n    key: 'val'\n  }\n}";
        var openBrace = source.IndexOf('{');

        // Act
        var result = BicepTextManipulationHelpers.FindClosingBrace(source, openBrace + 1);

        // Assert
        result.Should().Be(source.LastIndexOf('}'));
    }

    [Fact]
    public void Given_NoBraces_When_FindClosingBrace_Then_ReturnsMinusOne()
    {
        // Arrange
        const string source = "no braces here";

        // Act
        var result = BicepTextManipulationHelpers.FindClosingBrace(source, 0);

        // Assert
        result.Should().Be(-1);
    }

    [Fact]
    public void Given_DeeplyNestedBraces_When_FindClosingBrace_Then_FindsCorrectClosing()
    {
        // Arrange
        const string source = "{ { { } { } } }";
        // Start after the outer opening '{'
        var firstBrace = source.IndexOf('{');

        // Act — depth starts at 1, find matching outer close
        var result = BicepTextManipulationHelpers.FindClosingBrace(source, firstBrace + 1);

        // Assert
        result.Should().Be(source.Length - 1);
    }

    // ── Capitalize ──

    [Fact]
    public void Given_LowercaseString_When_Capitalize_Then_FirstCharIsUpper()
    {
        // Act
        var result = BicepTextManipulationHelpers.Capitalize("keyVault");

        // Assert
        result.Should().Be("KeyVault");
    }

    [Fact]
    public void Given_AlreadyCapitalized_When_Capitalize_Then_Unchanged()
    {
        // Act
        var result = BicepTextManipulationHelpers.Capitalize("KeyVault");

        // Assert
        result.Should().Be("KeyVault");
    }

    [Fact]
    public void Given_EmptyString_When_Capitalize_Then_ReturnsEmpty()
    {
        // Act
        var result = BicepTextManipulationHelpers.Capitalize(string.Empty);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void Given_SingleChar_When_Capitalize_Then_ReturnsUpperChar()
    {
        // Act
        var result = BicepTextManipulationHelpers.Capitalize("a");

        // Assert
        result.Should().Be("A");
    }

    // ── AddTypeImport ──

    [Fact]
    public void Given_NoImportLine_When_AddTypeImport_Then_PrependsImportStatement()
    {
        // Arrange
        const string moduleBicep = "param location string\nresource kv 'foo' = {}";
        const string typeName = "ManagedIdentityType";

        // Act
        var result = BicepTextManipulationHelpers.AddTypeImport(moduleBicep, typeName);

        // Assert
        result.Should().StartWith("import { ManagedIdentityType } from './types.bicep'");
        result.Should().Contain(moduleBicep);
    }

    [Fact]
    public void Given_ExistingImportLine_When_AddTypeImport_Then_AppendsToExistingImports()
    {
        // Arrange
        const string moduleBicep = "import { ExistingType } from './types.bicep'\nparam location string\n";
        const string typeName = "ManagedIdentityType";

        // Act
        var result = BicepTextManipulationHelpers.AddTypeImport(moduleBicep, typeName);

        // Assert
        result.Should().Contain("ExistingType, ManagedIdentityType");
        result.Should().Contain("from './types.bicep'");
    }

    [Fact]
    public void Given_TypeAlreadyImported_When_AddTypeImport_Then_NoChange()
    {
        // Arrange
        const string moduleBicep = "import { ManagedIdentityType } from './types.bicep'\nparam location string\n";

        // Act
        var result = BicepTextManipulationHelpers.AddTypeImport(moduleBicep, "ManagedIdentityType");

        // Assert
        result.Should().Be(moduleBicep);
    }
}
