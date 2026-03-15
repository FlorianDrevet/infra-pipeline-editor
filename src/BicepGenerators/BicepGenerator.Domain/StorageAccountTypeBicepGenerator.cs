namespace BicepGenerator.Domain;

public sealed class StorageAccountTypeBicepGenerator
    : IResourceTypeBicepGenerator
{
    public string ResourceType
        => "Microsoft.Storage/storageAccounts";

    public GeneratedTypeModule Generate(
        ResourceDefinition resource,
        EnvironmentDefinition environment)
    {
        return new GeneratedTypeModule
        {
            ModuleName = "storageAccount",
            ModuleFileName = "storageAccount.bicep",
            ModuleBicepContent = StorageAccountModuleTemplate,
            Parameters = new Dictionary<string, object>
            {
                ["location"] = environment.Location,
                ["name"] = resource.Name,
                ["sku"] = resource.Sku,
            }
        };
    }

    private const string StorageAccountModuleTemplate = """
        param location string
        param name string
        param sku string

        resource storage 'Microsoft.Storage/storageAccounts@2023-01-01' = {
          name: name
          location: location
          kind: 'StorageV2'
          sku: {
            name: sku
          }
        }
        """;
}
