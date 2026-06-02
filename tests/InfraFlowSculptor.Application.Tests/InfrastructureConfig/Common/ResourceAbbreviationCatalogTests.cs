using FluentAssertions;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.GenerationCore;
using Xunit;

namespace InfraFlowSculptor.Application.Tests.InfrastructureConfig.Common;

public sealed class ResourceAbbreviationCatalogTests
{
    [Theory]
    [InlineData(AzureResourceTypes.KeyVault, "kv")]
    [InlineData(AzureResourceTypes.RedisCache, "redis")]
    [InlineData(AzureResourceTypes.StorageAccount, "stg")]
    [InlineData(AzureResourceTypes.ResourceGroup, "rg")]
    [InlineData(AzureResourceTypes.AppServicePlan, "asp")]
    [InlineData(AzureResourceTypes.WebApp, "app")]
    [InlineData(AzureResourceTypes.FunctionApp, "func")]
    [InlineData(AzureResourceTypes.UserAssignedIdentity, "id")]
    [InlineData(AzureResourceTypes.AppConfiguration, "appcs")]
    [InlineData(AzureResourceTypes.ContainerAppEnvironment, "cae")]
    [InlineData(AzureResourceTypes.ContainerApp, "ca")]
    [InlineData(AzureResourceTypes.LogAnalyticsWorkspace, "law")]
    [InlineData(AzureResourceTypes.ApplicationInsights, "appi")]
    [InlineData(AzureResourceTypes.CosmosDb, "cosmos")]
    [InlineData(AzureResourceTypes.SqlServer, "sql")]
    [InlineData(AzureResourceTypes.SqlDatabase, "sqldb")]
    [InlineData(AzureResourceTypes.ServiceBusNamespace, "sb")]
    [InlineData(AzureResourceTypes.ContainerRegistry, "acr")]
    [InlineData(AzureResourceTypes.EventHubNamespace, "evhns")]
    [InlineData(AzureResourceTypes.DocumentIntelligence, "docint")]
    [InlineData(AzureResourceTypes.VirtualNetwork, "vnet")]
    public void Given_KnownResourceType_When_GetAbbreviation_Then_ReturnsCorrectAbbreviation(
        string resourceType, string expectedAbbreviation)
    {
        var result = ResourceAbbreviationCatalog.GetAbbreviation(resourceType);

        result.Should().Be(expectedAbbreviation, $"'{resourceType}' should map to '{expectedAbbreviation}'");
    }

    [Fact]
    public void Given_UnknownResourceType_When_GetAbbreviation_Then_ReturnsLowercaseTypeName()
    {
        const string unknownType = "SomeUnknownResource";

        var result = ResourceAbbreviationCatalog.GetAbbreviation(unknownType);

        result.Should().Be("someunknownresource");
    }

    [Fact]
    public void Given_ResourceTypeLookup_When_CasingVaries_Then_ReturnsAbbreviation()
    {
        var result = ResourceAbbreviationCatalog.GetAbbreviation("virtualnetwork");

        result.Should().Be("vnet");
    }

    [Fact]
    public void Given_VirtualNetwork_When_GetAbbreviation_Then_ReturnsVnet()
    {
        var result = ResourceAbbreviationCatalog.GetAbbreviation(AzureResourceTypes.VirtualNetwork);

        result.Should().Be("vnet");
    }

    [Fact]
    public void Given_GetAll_When_Called_Then_ContainsVirtualNetworkEntry()
    {
        var all = ResourceAbbreviationCatalog.GetAll();

        all.Should().ContainKey(AzureResourceTypes.VirtualNetwork);
        all[AzureResourceTypes.VirtualNetwork].Should().Be("vnet");
    }
}
