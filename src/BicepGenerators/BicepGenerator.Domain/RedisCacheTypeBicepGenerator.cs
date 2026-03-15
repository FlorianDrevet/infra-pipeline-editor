namespace BicepGenerator.Domain;

public sealed class RedisCacheTypeBicepGenerator
    : IResourceTypeBicepGenerator
{
    public string ResourceType
        => "Microsoft.Cache/Redis";

    public GeneratedTypeModule Generate(
        IReadOnlyCollection<ResourceDefinition> resources,
        EnvironmentDefinition environment)
    {
        return new GeneratedTypeModule
        {
            ModuleName = "redisCaches",
            ModuleFileName = "redisCaches.bicep",
            ModuleBicepContent = RedisCacheModuleTemplate,
            Parameters = new Dictionary<string, object>
            {
                ["location"] = environment.Location,
                ["redisCaches"] = resources.Select(r => new
                {
                    name = r.Name,
                    skuName = r.Properties.GetValueOrDefault("skuName", "Basic"),
                    skuFamily = r.Properties.GetValueOrDefault("skuFamily", "C"),
                    capacity = int.TryParse(r.Properties.GetValueOrDefault("capacity", "1"), out var cap) ? cap : 1,
                    redisVersion = r.Properties.GetValueOrDefault("redisVersion", "6"),
                    enableNonSslPort = r.Properties.GetValueOrDefault("enableNonSslPort", "false") == "true",
                    minimumTlsVersion = r.Properties.GetValueOrDefault("minimumTlsVersion", "1.2"),
                }).ToList<object>()
            }
        };
    }

    private const string RedisCacheModuleTemplate = """
        param location string
        param redisCaches array

        resource redis 'Microsoft.Cache/Redis@2023-08-01' = [
          for cache in redisCaches: {
            name: cache.name
            location: location
            properties: {
              sku: {
                name: cache.skuName
                family: cache.skuFamily
                capacity: cache.capacity
              }
              redisVersion: cache.redisVersion
              enableNonSslPort: cache.enableNonSslPort
              minimumTlsVersion: cache.minimumTlsVersion
            }
          }
        ]
        """;
}
