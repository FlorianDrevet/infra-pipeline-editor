using FluentAssertions;
using InfraFlowSculptor.Domain.Common.AzureRoleDefinitions;

namespace InfraFlowSculptor.Domain.Tests.Common.AzureRoleDefinitions;

public sealed class AzureRoleDefinitionCatalogTests
{
    private const string KeyVaultResourceType = "KeyVault";
    private const string WebAppResourceType = "WebApp";
    private const string UnknownResourceType = "Unknown";

    [Fact]
    public void Given_KeyVaultResourceType_When_GetForResourceType_Then_ReturnsKnownSecretAccessRoles()
    {
        // Act
        var roles = AzureRoleDefinitionCatalog.GetForResourceType(KeyVaultResourceType);

        // Assert
        roles.Should().Contain(role =>
            role.Id == AzureRoleDefinitionCatalog.KeyVaultAdministrator
            && role.Name == "Key Vault Administrator");
        roles.Should().Contain(role =>
            role.Id == AzureRoleDefinitionCatalog.KeyVaultSecretsUser
            && role.Name == "Key Vault Secrets User");
    }

    [Fact]
    public void Given_WebAppResourceType_When_GetForResourceType_Then_ReturnsSharedManagementRoles()
    {
        // Act
        var roles = AzureRoleDefinitionCatalog.GetForResourceType(WebAppResourceType);

        // Assert
        roles.Should().Contain(role =>
            role.Id == "de139f84-1756-47ae-9be6-808fbbe84772"
            && role.Name == "Website Contributor");
        roles.Should().Contain(role =>
            role.Id == "b24988ac-6180-42a0-ab88-20f7382dd24c"
            && role.Name == "Contributor");
        roles.Should().Contain(role =>
            role.Id == "acdd72a7-3385-48ef-bd42-f606fba81ae7"
            && role.Name == "Reader");
    }

    [Fact]
    public void Given_UnknownResourceType_When_GetForResourceType_Then_ReturnsEmpty()
    {
        // Act
        var roles = AzureRoleDefinitionCatalog.GetForResourceType(UnknownResourceType);

        // Assert
        roles.Should().BeEmpty();
    }

    [Fact]
    public void Given_ReaderRoleDefinition_When_IsValidForResourceType_Then_ReturnsTrue()
    {
        // Act
        var isValid = AzureRoleDefinitionCatalog.IsValidForResourceType(
            WebAppResourceType,
            "acdd72a7-3385-48ef-bd42-f606fba81ae7");

        // Assert
        isValid.Should().BeTrue();
    }
}