using InfraFlowSculptor.BicepGeneration.Models;

namespace InfraFlowSculptor.BicepGeneration.Generators;

/// <summary>
/// Generates a Bicep module for Azure Cosmos DB (<c>Microsoft.DocumentDB/databaseAccounts@2024-05-15</c>).
/// </summary>
public sealed class CosmosDbTypeBicepGenerator
    : IResourceTypeBicepGenerator
{
    /// <inheritdoc />
    public string ResourceType
        => "Microsoft.DocumentDB/databaseAccounts";

    /// <inheritdoc />
    public string ResourceTypeName => "CosmosDb";

    /// <inheritdoc />
    public GeneratedTypeModule Generate(ResourceDefinition resource)
    {
        return new GeneratedTypeModule
        {
            ModuleName = "cosmosDb",
            ModuleFileName = "cosmosDb.bicep",
            ModuleBicepContent = CosmosDbModuleTemplate,
            ResourceTypeName = ResourceTypeName,
            Parameters = new Dictionary<string, object>()
        };
    }

    private const string CosmosDbModuleTemplate = """
        param location string
        param name string
        param kind string = 'GlobalDocumentDB'
        param consistencyLevel string = 'Session'
        param maxStalenessPrefix int = 100
        param maxIntervalInSeconds int = 5
        param enableAutomaticFailover bool = false
        param enableMultipleWriteLocations bool = false
        param backupPolicyType string = 'Periodic'
        param enableFreeTier bool = false
        param capabilities array = []

        resource cosmosDbAccount 'Microsoft.DocumentDB/databaseAccounts@2024-05-15' = {
          name: name
          location: location
          kind: kind
          properties: {
            databaseAccountOfferType: 'Standard'
            consistencyPolicy: {
              defaultConsistencyLevel: consistencyLevel
              maxStalenessPrefix: maxStalenessPrefix
              maxIntervalInSeconds: maxIntervalInSeconds
            }
            enableAutomaticFailover: enableAutomaticFailover
            enableMultipleWriteLocations: enableMultipleWriteLocations
            backupPolicy: {
              type: backupPolicyType
            }
            enableFreeTier: enableFreeTier
            capabilities: capabilities
            locations: [
              {
                locationName: location
                failoverPriority: 0
                isZoneRedundant: false
              }
            ]
          }
        }
        """;
}
