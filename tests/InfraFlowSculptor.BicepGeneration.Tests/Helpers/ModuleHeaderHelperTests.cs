using FluentAssertions;
using InfraFlowSculptor.BicepGeneration.Helpers;
using InfraFlowSculptor.GenerationCore;

namespace InfraFlowSculptor.BicepGeneration.Tests.Helpers;

public sealed class ModuleHeaderHelperTests
{
    [Fact]
    public void Given_KnownResourceType_When_GenerateModuleHeader_Then_ContainsDisplayNameAndDocUrl()
    {
        // Act
        var result = ModuleHeaderHelper.GenerateModuleHeader(
            AzureResourceTypes.KeyVault, "keyVault.module.bicep");

        // Assert
        result.Should().Contain("Key Vault Module");
        result.Should().Contain("Module: keyVault.module.bicep");
        result.Should().Contain("Deploys an Azure Key Vault resource");
        result.Should().Contain("https://learn.microsoft.com/");
        result.Should().Contain("// ====");
    }

    [Fact]
    public void Given_UnknownResourceType_When_GenerateModuleHeader_Then_OmitsDocUrl()
    {
        // Act
        var result = ModuleHeaderHelper.GenerateModuleHeader(
            "CustomWidget", "customWidget.module.bicep");

        // Assert
        result.Should().Contain("CustomWidget Module");
        result.Should().Contain("Module: customWidget.module.bicep");
        result.Should().NotContain("See: https://");
    }

    [Fact]
    public void Given_BicepContent_When_AddModuleHeader_Then_PrependedHeader()
    {
        // Arrange
        const string content = "param name string";

        // Act
        var result = ModuleHeaderHelper.AddModuleHeader(
            AzureResourceTypes.StorageAccount, "storageAccount.module.bicep", content);

        // Assert
        result.Should().StartWith("// ====");
        result.Should().EndWith("param name string");
        result.Should().Contain("Storage Account Module");
    }

    [Fact]
    public void Given_NewAdditionalContent_When_MergeTypesContent_Then_AppendsContent()
    {
        // Arrange
        const string existing = "type Sku = 'Standard' | 'Premium'";
        const string additional = "type Tier = 'Basic' | 'Advanced'";

        // Act
        var result = ModuleHeaderHelper.MergeTypesContent(existing, additional);

        // Assert
        result.Should().Contain("type Sku = 'Standard' | 'Premium'");
        result.Should().Contain("type Tier = 'Basic' | 'Advanced'");
    }

    [Fact]
    public void Given_DuplicateContent_When_MergeTypesContent_Then_ReturnsExistingOnly()
    {
        // Arrange
        const string existing = "type Sku = 'Standard' | 'Premium'";
        const string duplicate = "type Sku = 'Standard' | 'Premium'";

        // Act
        var result = ModuleHeaderHelper.MergeTypesContent(existing, duplicate);

        // Assert
        result.Should().Be(existing);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Given_EmptyAdditionalContent_When_MergeTypesContent_Then_ReturnsExisting(
        string additional)
    {
        // Arrange
        const string existing = "type Sku = 'Standard' | 'Premium'";

        // Act
        var result = ModuleHeaderHelper.MergeTypesContent(existing, additional);

        // Assert
        result.Should().Be(existing);
    }
}
