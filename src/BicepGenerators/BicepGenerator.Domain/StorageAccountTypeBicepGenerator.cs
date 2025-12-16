namespace BicepGenerator.Domain;

public sealed class StorageAccountTypeBicepGenerator 
    : IResourceTypeBicepGenerator
{
    public string ResourceType 
        => "Microsoft.Storage/storageAccounts";

    public GeneratedTypeModule Generate(
        IReadOnlyCollection<ResourceDefinition> resources,
        EnvironmentDefinition environment)
    {
        return new GeneratedTypeModule
        {
            ModuleName = "storageAccounts",
            ModuleFileName = "storageAccounts.bicep",
            ModuleBicepContent = StorageAccountModuleTemplate,
            Parameters = new Dictionary<string, object>
            {
                ["location"] = environment.Location,
                ["storageAccounts"] = resources.Select(r => new
                {
                    name = r.Name,
                    sku = r.Sku,
                }).ToArray()
            }
        };
    }

    private const string StorageAccountModuleTemplate = """
                                                        param location string
                                                        param storageAccounts array

                                                        resource storage 'Microsoft.Storage/storageAccounts@2023-01-01' = [
                                                          for sa in storageAccounts: {
                                                            name: sa.name
                                                            location: location
                                                            kind: 'StorageV2'
                                                            sku: {
                                                              name: sa.sku
                                                            }
                                                            properties: {
                                                              accessTier: sa.accessTier
                                                            }
                                                          }
                                                        ]
                                                        """;
}
