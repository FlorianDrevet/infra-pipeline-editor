using FluentAssertions;
using InfraFlowSculptor.BicepGeneration.Helpers;
using InfraFlowSculptor.GenerationCore;

namespace InfraFlowSculptor.BicepGeneration.Tests.Helpers;

public sealed class ResourceTypeMetadataTests
{
    [Theory]
    [InlineData(AzureResourceTypes.KeyVault, AzureResourceTypes.KeyVault)]
    [InlineData(AzureResourceTypes.RedisCache, AzureResourceTypes.RedisCache)]
    [InlineData(AzureResourceTypes.StorageAccount, AzureResourceTypes.StorageAccount)]
    [InlineData(AzureResourceTypes.WebApp, AzureResourceTypes.WebApp)]
    [InlineData(AzureResourceTypes.SqlServer, AzureResourceTypes.SqlServer)]
    [InlineData(AzureResourceTypes.CosmosDb, AzureResourceTypes.CosmosDb)]
    public void Given_KnownResourceType_When_GetModuleFolderName_Then_ReturnsSameTypeName(
        string resourceType, string expected)
    {
        // Act
        var result = ResourceTypeMetadata.GetModuleFolderName(resourceType);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void Given_UnknownResourceType_When_GetModuleFolderName_Then_ReturnsInputAsIs()
    {
        // Arrange
        const string unknown = "SomeUnknownType";

        // Act
        var result = ResourceTypeMetadata.GetModuleFolderName(unknown);

        // Assert
        result.Should().Be(unknown);
    }

    [Theory]
    [InlineData(AzureResourceTypes.KeyVault, "Key Vault")]
    [InlineData(AzureResourceTypes.RedisCache, "Redis Cache")]
    [InlineData(AzureResourceTypes.StorageAccount, "Storage Account")]
    [InlineData(AzureResourceTypes.WebApp, "Web App")]
    [InlineData(AzureResourceTypes.FunctionApp, "Function App")]
    [InlineData(AzureResourceTypes.CosmosDb, "Cosmos DB")]
    [InlineData(AzureResourceTypes.SqlServer, "SQL Server")]
    [InlineData(AzureResourceTypes.SqlDatabase, "SQL Database")]
    [InlineData(AzureResourceTypes.ServiceBusNamespace, "Service Bus Namespace")]
    [InlineData(AzureResourceTypes.ApplicationInsights, "Application Insights")]
    public void Given_KnownResourceType_When_GetResourceTypeDisplayName_Then_ReturnsHumanReadableName(
        string resourceType, string expected)
    {
        // Act
        var result = ResourceTypeMetadata.GetResourceTypeDisplayName(resourceType);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void Given_UnknownResourceType_When_GetResourceTypeDisplayName_Then_ReturnsInputAsIs()
    {
        // Arrange
        const string unknown = "CustomWidget";

        // Act
        var result = ResourceTypeMetadata.GetResourceTypeDisplayName(unknown);

        // Assert
        result.Should().Be(unknown);
    }

    [Theory]
    [InlineData(AzureResourceTypes.ArmTypes.KeyVaultType)]
    [InlineData(AzureResourceTypes.ArmTypes.SqlServerType)]
    [InlineData(AzureResourceTypes.ArmTypes.StorageAccountType)]
    [InlineData(AzureResourceTypes.ArmTypes.ContainerRegistryType)]
    [InlineData(AzureResourceTypes.ArmTypes.EventHubNamespaceType)]
    public void Given_KnownArmType_When_GetExistingResourceApiVersion_Then_ReturnsNonEmptyVersion(
        string armType)
    {
        // Act
        var result = ResourceTypeMetadata.GetExistingResourceApiVersion(armType);

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().MatchRegex(@"^\d{4}-\d{2}-\d{2}");
    }

    [Theory]
    [InlineData(AzureResourceTypes.ArmTypes.KeyVaultType, "keyVault")]
    [InlineData(AzureResourceTypes.ArmTypes.RedisCacheType, "redisCache")]
    [InlineData(AzureResourceTypes.ArmTypes.StorageAccountType, "storageAccount")]
    [InlineData(AzureResourceTypes.ArmTypes.WebAppType, "webApp")]
    [InlineData(AzureResourceTypes.ArmTypes.FunctionAppType, "functionApp")]
    [InlineData(AzureResourceTypes.ArmTypes.CosmosDbType, "cosmosDb")]
    [InlineData(AzureResourceTypes.ArmTypes.SqlServerType, "sqlServer")]
    [InlineData(AzureResourceTypes.ArmTypes.SqlDatabaseType, "sqlDatabase")]
    [InlineData(AzureResourceTypes.ArmTypes.ServiceBusNamespaceType, "serviceBusNamespace")]
    [InlineData(AzureResourceTypes.ArmTypes.ContainerRegistryType, "containerRegistry")]
    [InlineData(AzureResourceTypes.ArmTypes.EventHubNamespaceType, "eventHubNamespace")]
    public void Given_KnownArmType_When_GetBaseModuleName_Then_ReturnsCamelCaseName(
        string armType, string expected)
    {
        // Act
        var result = ResourceTypeMetadata.GetBaseModuleName(armType);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void Given_UnknownArmType_When_GetBaseModuleName_Then_ReturnsUnknown()
    {
        // Act
        var result = ResourceTypeMetadata.GetBaseModuleName("Microsoft.Unknown/things");

        // Assert
        result.Should().Be("unknown");
    }

    [Theory]
    [InlineData(AzureResourceTypes.KeyVault)]
    [InlineData(AzureResourceTypes.StorageAccount)]
    [InlineData(AzureResourceTypes.SqlServer)]
    public void Given_KnownResourceType_When_GetResourceTypeDocumentationUrl_Then_ReturnsLearnUrl(
        string resourceType)
    {
        // Act
        var result = ResourceTypeMetadata.GetResourceTypeDocumentationUrl(resourceType);

        // Assert
        result.Should().StartWith("https://learn.microsoft.com/");
    }

    [Fact]
    public void Given_UnknownResourceType_When_GetResourceTypeDocumentationUrl_Then_ReturnsEmpty()
    {
        // Act
        var result = ResourceTypeMetadata.GetResourceTypeDocumentationUrl("UnknownType");

        // Assert
        result.Should().BeEmpty();
    }
}
