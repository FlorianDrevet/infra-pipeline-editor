using FluentAssertions;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.OwnedEntities;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.KeyVaultAggregate;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using InfraFlowSculptor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InfraFlowSculptor.Infrastructure.Tests.Persistence.Configurations;

public sealed class AzureResourcePrivateEndpointConfigurationTests
{
    [Fact]
    public async Task Given_ResourceWithPrivateEndpointConfiguration_When_Reloaded_Then_ConfigurationIsPreserved_Async()
    {
        // Arrange
        const string subnetName = "snet-private-endpoints";
        const string dnsHubResourceGroupId = "/subscriptions/11111111-1111-1111-1111-111111111111/resourceGroups/rg-dns-hub";
        const string dnsHubSubscriptionId = "11111111-1111-1111-1111-111111111111";
        var databaseName = $"private_endpoint_config_{Guid.NewGuid()}";
        var virtualNetworkId = AzureResourceId.CreateUnique();
        var keyVault = KeyVault.Create(
            ResourceGroupId.CreateUnique(),
            new Name("kv-private-config"),
            new Location(Location.LocationEnum.WestEurope));
        keyVault.ConfigurePrivateEndpoint(PrivateEndpointConfiguration.ExistingHub(
            virtualNetworkId,
            new Name(subnetName),
            dnsHubResourceGroupId,
            dnsHubSubscriptionId));

        await using (var arrangeContext = CreateInMemoryContext(databaseName))
        {
            arrangeContext.KeyVaults.Add(keyVault);
            await arrangeContext.SaveChangesAsync();
        }

        // Act
        await using var assertContext = CreateInMemoryContext(databaseName);
        var stored = await assertContext.KeyVaults
            .AsNoTracking()
            .SingleAsync(entity => entity.Id == keyVault.Id);

        // Assert
        stored.IsPrivatized.Should().BeTrue();
        stored.PrivateEndpointConfiguration.Should().NotBeNull();
        stored.PrivateEndpointConfiguration!.VirtualNetworkId.Should().Be(virtualNetworkId);
        stored.PrivateEndpointConfiguration.SubnetName.Value.Should().Be(subnetName);
        stored.PrivateEndpointConfiguration.DnsMode.Value.Should().Be(PrivateEndpointDnsMode.Mode.ExistingHub);
        stored.PrivateEndpointConfiguration.DnsHubResourceGroupId.Should().Be(dnsHubResourceGroupId);
        stored.PrivateEndpointConfiguration.DnsHubSubscriptionId.Should().Be(dnsHubSubscriptionId);
    }

    private static ProjectDbContext CreateInMemoryContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<ProjectDbContext>()
            .UseInMemoryDatabase(databaseName)
            .ConfigureWarnings(builder => builder.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new ProjectDbContext(options);
    }
}