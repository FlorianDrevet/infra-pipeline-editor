using InfraFlowSculptor.BicepGeneration.Models;

namespace InfraFlowSculptor.BicepGeneration.Generators;

public sealed class RedisCacheTypeBicepGenerator
    : IResourceTypeBicepGenerator
{
    public string ResourceType
        => "Microsoft.Cache/Redis";

    /// <inheritdoc />
    public string ResourceTypeName => "RedisCache";

    public GeneratedTypeModule Generate(ResourceDefinition resource)
    {
        return new GeneratedTypeModule
        {
            ModuleName = "redisCache",
            ModuleFileName = "redisCache.bicep",
            ModuleBicepContent = RedisCacheModuleTemplate,
            ResourceTypeName = ResourceTypeName,
            Parameters = new Dictionary<string, object>
            {
                ["skuName"] = resource.Properties.GetValueOrDefault("skuName", "Basic"),
                ["skuFamily"] = resource.Properties.GetValueOrDefault("skuFamily", "C"),
                ["capacity"] = int.TryParse(resource.Properties.GetValueOrDefault("capacity", "1"), out var cap) ? cap : 1,
                ["redisVersion"] = resource.Properties.GetValueOrDefault("redisVersion", "6"),
                ["enableNonSslPort"] = resource.Properties.GetValueOrDefault("enableNonSslPort", "false") == "true",
                ["minimumTlsVersion"] = resource.Properties.GetValueOrDefault("minimumTlsVersion", "1.2"),
            }
        };
    }

    private const string RedisCacheModuleTemplate = """
        param location string
        param name string
        param skuName string
        param skuFamily string
        param capacity int
        param redisVersion string
        param enableNonSslPort bool
        param minimumTlsVersion string

        resource redis 'Microsoft.Cache/Redis@2023-08-01' = {
          name: name
          location: location
          properties: {
            sku: {
              name: skuName
              family: skuFamily
              capacity: capacity
            }
            redisVersion: redisVersion
            enableNonSslPort: enableNonSslPort
            minimumTlsVersion: minimumTlsVersion
          }
        }
        """;
}
