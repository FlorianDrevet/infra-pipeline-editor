namespace BicepGenerator.Domain;

public sealed class KeyVaultTypeBicepGenerator
    : IResourceTypeBicepGenerator
{
    public string ResourceType
        => "Microsoft.KeyVault/vaults";

    public GeneratedTypeModule Generate(
        IReadOnlyCollection<ResourceDefinition> resources,
        EnvironmentDefinition environment)
    {
        return new GeneratedTypeModule
        {
            ModuleName = "keyVaults",
            ModuleFileName = "keyVaults.bicep",
            ModuleBicepContent = KeyVaultModuleTemplate,
            Parameters = new Dictionary<string, object>
            {
                ["location"] = environment.Location,
                ["keyVaults"] = resources.Select(r => new
                {
                    name = r.Name,
                    sku = r.Sku.ToLower(),
                }).ToList<object>()
            }
        };
    }

    private const string KeyVaultModuleTemplate = """
        param location string
        param keyVaults array

        resource kv 'Microsoft.KeyVault/vaults@2023-07-01' = [
          for vault in keyVaults: {
            name: vault.name
            location: location
            properties: {
              sku: {
                family: 'A'
                name: vault.sku
              }
              tenantId: subscription().tenantId
              enabledForDeployment: true
              enabledForTemplateDeployment: true
              enableSoftDelete: true
            }
          }
        ]
        """;
}
