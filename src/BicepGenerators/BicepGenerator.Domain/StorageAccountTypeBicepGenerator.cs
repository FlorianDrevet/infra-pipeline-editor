namespace BicepGenerator.Domain;

public sealed class StorageAccountTypeBicepGenerator
    : IResourceTypeBicepGenerator
{
    public string ResourceType
        => "Microsoft.Storage/storageAccounts";

    /// <inheritdoc />
    public string ResourceTypeName => "StorageAccount";

    public GeneratedTypeModule Generate(ResourceDefinition resource)
    {
        return new GeneratedTypeModule
        {
            ModuleName = "storageAccount",
            ModuleFileName = "storageAccount.bicep",
            ModuleBicepContent = StorageAccountModuleTemplate,
            ResourceTypeName = ResourceTypeName,
            Parameters = new Dictionary<string, object>
            {
                ["sku"] = resource.Properties.GetValueOrDefault("sku", "Standard_LRS"),
                ["kind"] = resource.Properties.GetValueOrDefault("kind", "StorageV2"),
                ["accessTier"] = resource.Properties.GetValueOrDefault("accessTier", "Hot"),
                ["allowBlobPublicAccess"] = resource.Properties.GetValueOrDefault("allowBlobPublicAccess", "false") == "true",
                ["supportsHttpsTrafficOnly"] = resource.Properties.GetValueOrDefault("supportsHttpsTrafficOnly", "true") == "true",
                ["minimumTlsVersion"] = resource.Properties.GetValueOrDefault("minimumTlsVersion", "TLS1_2"),
            }
        };
    }

    private const string StorageAccountModuleTemplate = """
        param location string
        param name string
        param sku string
        param kind string
        param accessTier string
        param allowBlobPublicAccess bool
        param supportsHttpsTrafficOnly bool
        param minimumTlsVersion string

        var storagePropertiesBase = {
          allowBlobPublicAccess: allowBlobPublicAccess
          supportsHttpsTrafficOnly: supportsHttpsTrafficOnly
          minimumTlsVersion: minimumTlsVersion
        }

        var storageAccessTierProperties = contains([
          'BlobStorage'
          'StorageV2'
        ], kind) ? {
          accessTier: accessTier
        } : {}

        resource storage 'Microsoft.Storage/storageAccounts@2023-01-01' = {
          name: name
          location: location
          kind: kind
          sku: {
            name: sku
          }
          properties: union(storagePropertiesBase, storageAccessTierProperties)
        }
        """;
}
