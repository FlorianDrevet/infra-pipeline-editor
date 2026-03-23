namespace BicepGenerator.Domain;

public sealed class KeyVaultTypeBicepGenerator
    : IResourceTypeBicepGenerator
{
    public string ResourceType
        => "Microsoft.KeyVault/vaults";

    /// <inheritdoc />
    public string ResourceTypeName => "KeyVault";

    public GeneratedTypeModule Generate(ResourceDefinition resource)
    {
        return new GeneratedTypeModule
        {
            ModuleName = "keyVault",
            ModuleFileName = "keyVault.bicep",
            ModuleBicepContent = KeyVaultModuleTemplate,
            ResourceTypeName = ResourceTypeName,
            Parameters = new Dictionary<string, object>
            {
                ["sku"] = resource.Sku.ToLower(),
            }
        };
    }

    private const string KeyVaultModuleTemplate = """
        param location string
        param name string
        param sku string

        resource kv 'Microsoft.KeyVault/vaults@2023-07-01' = {
          name: name
          location: location
          properties: {
            sku: {
              family: 'A'
              name: sku
            }
            tenantId: subscription().tenantId
            enabledForDeployment: true
            enabledForTemplateDeployment: true
            enableSoftDelete: true
          }
        }
        """;
}
