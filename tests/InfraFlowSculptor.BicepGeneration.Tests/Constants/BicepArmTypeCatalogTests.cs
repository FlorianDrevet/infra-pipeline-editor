using FluentAssertions;
using InfraFlowSculptor.BicepGeneration.Constants;
using InfraFlowSculptor.BicepGeneration.Generators;
using InfraFlowSculptor.BicepGeneration.Helpers;
using InfraFlowSculptor.GenerationCore;

namespace InfraFlowSculptor.BicepGeneration.Tests.Constants;

public sealed class BicepArmTypeCatalogTests
{
    [Fact]
    public void Given_KnownArmTypes_When_ReadingCatalog_Then_ReturnsQualifiedTypesWithCentralizedVersions()
    {
        BicepArmTypeCatalog.WebAppArmType.Should().Be("Microsoft.Web/sites@2023-12-01");
        BicepArmTypeCatalog.HostNameBindingsArmType.Should().Be("Microsoft.Web/sites/hostNameBindings@2023-12-01");
        BicepArmTypeCatalog.StorageAccountArmType.Should().Be("Microsoft.Storage/storageAccounts@2025-06-01");
        BicepArmTypeCatalog.StorageBlobServicesArmType.Should().Be("Microsoft.Storage/storageAccounts/blobServices@2025-06-01");
        BicepArmTypeCatalog.RoleAssignmentsArmType.Should().Be("Microsoft.Authorization/roleAssignments@2022-04-01");
    }

    [Fact]
    public void Given_KnownResourceTypes_When_ResolvingMetadataVersions_Then_UsesCentralCatalog()
    {
        ResourceTypeMetadata.GetExistingResourceApiVersion(AzureResourceTypes.ArmTypes.KeyVaultType)
            .Should().Be(BicepArmTypeCatalog.KeyVaultApiVersion);

        ResourceTypeMetadata.GetExistingResourceApiVersion(AzureResourceTypes.ArmTypes.SqlServerType)
            .Should().Be(BicepArmTypeCatalog.SqlServerApiVersion);

        ResourceTypeMetadata.GetExistingResourceApiVersion(AzureResourceTypes.ArmTypes.ContainerRegistryType)
            .Should().Be(BicepArmTypeCatalog.ContainerRegistryApiVersion);

        ResourceTypeMetadata.GetExistingResourceApiVersion(AzureResourceTypes.ArmTypes.EventHubNamespaceType)
            .Should().Be(BicepArmTypeCatalog.EventHubNamespaceApiVersion);

        RoleAssignmentModuleTemplates.GetMetadata(AzureResourceTypes.ContainerRegistry)!
            .ApiVersion.Should().Be(BicepArmTypeCatalog.ContainerRegistryApiVersion);

        RoleAssignmentModuleTemplates.GetMetadata(AzureResourceTypes.EventHubNamespace)!
            .ApiVersion.Should().Be(BicepArmTypeCatalog.EventHubNamespaceApiVersion);
    }
}
