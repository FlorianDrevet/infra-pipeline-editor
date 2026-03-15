using BicepGenerator.Application.Common.Interfaces.Persistence;
using BicepGenerator.Application.InfrastructureConfig.ReadModels;
using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.KeyVaultAggregate;
using InfraFlowSculptor.Domain.RedisCacheAggregate;
using InfraFlowSculptor.Domain.RedisCacheAggregate.ValueObjects;
using Microsoft.EntityFrameworkCore;
using InfraFlowSculptorDbContext = InfraFlowSculptor.Infrastructure.Persistence.ProjectDbContext;

namespace BicepGenerator.Infrastructure.Persistence.Repositories;

public class InfrastructureConfigReadRepository(InfraFlowSculptorDbContext dbContext)
    : IInfrastructureConfigReadRepository
{
    public async Task<InfrastructureConfigReadModel?> GetByIdWithResourcesAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var configId = new InfrastructureConfigId(id);

        var config = await dbContext.InfrastructureConfigs
            .Include(c => c.ResourceGroups)
                .ThenInclude(rg => rg.Resources)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == configId, cancellationToken);

        if (config is null)
            return null;

        var resourceGroups = config.ResourceGroups.Select(rg =>
        {
            var resources = rg.Resources
                .Select(MapResource)
                .OfType<AzureResourceReadModel>()
                .ToList();

            return new ResourceGroupReadModel(
                rg.Id.Value,
                rg.Name.Value,
                MapLocation(rg.Location),
                resources);
        }).ToList();

        var environments = config.EnvironmentDefinitions.Select(e =>
            new EnvironmentDefinitionReadModel(
                e.Id.Value,
                e.Name.Value,
                MapLocation(e.Location))).ToList();

        return new InfrastructureConfigReadModel(
            config.Id.Value,
            config.Name.Value,
            resourceGroups,
            environments);
    }

    /// <summary>
    /// Maps an <see cref="AzureResource"/> to its read model.
    /// Returns <c>null</c> for resource types that are not yet supported by the generator,
    /// so that callers can filter them out rather than producing invalid Bicep output.
    /// </summary>
    private static AzureResourceReadModel? MapResource(AzureResource resource)
    {
        return resource switch
        {
            KeyVault kv => new AzureResourceReadModel(
                kv.Id.Value,
                kv.Name.Value,
                MapLocation(kv.Location),
                "Microsoft.KeyVault/vaults",
                new Dictionary<string, string>
                {
                    ["sku"] = kv.Sku.Value.ToString().ToLower()
                }),
            RedisCache rc => new AzureResourceReadModel(
                rc.Id.Value,
                rc.Name.Value,
                MapLocation(rc.Location),
                "Microsoft.Cache/Redis",
                new Dictionary<string, string>
                {
                    ["skuName"] = rc.Sku.Value.ToString(),
                    ["skuFamily"] = rc.Sku.Value == RedisCacheSku.Sku.Premium ? "P" : "C",
                    ["capacity"] = rc.Capacity.ToString(),
                    ["redisVersion"] = rc.RedisVersion.ToString(),
                    ["enableNonSslPort"] = rc.EnableNonSslPort.ToString().ToLower(),
                    ["minimumTlsVersion"] = MapTlsVersion(rc.MinimumTlsVersion),
                }),
            _ => null
        };
    }

    private static string MapTlsVersion(TlsVersion tlsVersion)
    {
        return tlsVersion.Value switch
        {
            TlsVersion.Version.Tls10 => "1.0",
            TlsVersion.Version.Tls11 => "1.1",
            TlsVersion.Version.Tls12 => "1.2",
            _ => throw new ArgumentOutOfRangeException(nameof(tlsVersion),
                $"Unsupported TLS version: {tlsVersion.Value}")
        };
    }

    private static string MapLocation(Location location)
    {
        return location.Value switch
        {
            Location.LocationEnum.EastUS => "eastus",
            Location.LocationEnum.WestUS => "westus",
            Location.LocationEnum.CentralUS => "centralus",
            Location.LocationEnum.NorthEurope => "northeurope",
            Location.LocationEnum.WestEurope => "westeurope",
            Location.LocationEnum.SoutheastAsia => "southeastasia",
            Location.LocationEnum.EastAsia => "eastasia",
            Location.LocationEnum.AustraliaEast => "australiaeast",
            Location.LocationEnum.JapanEast => "japaneast",
            _ => "westeurope"
        };
    }
}
