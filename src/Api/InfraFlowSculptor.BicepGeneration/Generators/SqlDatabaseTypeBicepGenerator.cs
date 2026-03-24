using InfraFlowSculptor.BicepGeneration.Models;

namespace InfraFlowSculptor.BicepGeneration.Generators;

/// <summary>Generates Bicep module for Azure SQL Database (<c>Microsoft.Sql/servers/databases@2023-08-01-preview</c>).</summary>
public sealed class SqlDatabaseTypeBicepGenerator
    : IResourceTypeBicepGenerator
{
    /// <inheritdoc />
    public string ResourceType
        => "Microsoft.Sql/servers/databases";

    /// <inheritdoc />
    public string ResourceTypeName => "SqlDatabase";

    /// <inheritdoc />
    public GeneratedTypeModule Generate(ResourceDefinition resource)
    {
        var sku = resource.Properties.GetValueOrDefault("sku", "Basic");
        var maxSizeGb = int.TryParse(resource.Properties.GetValueOrDefault("maxSizeGb", "2"), out var sz) ? sz : 2;

        return new GeneratedTypeModule
        {
            ModuleName = "sqlDatabase",
            ModuleFileName = "sqlDatabase.bicep",
            ModuleFolderName = "SqlDatabase",
            ModuleBicepContent = SqlDatabaseModuleTemplate,
            ModuleTypesBicepContent = SqlDatabaseTypesTemplate,
            ResourceTypeName = ResourceTypeName,
            Parameters = new Dictionary<string, object>
            {
                ["sku"] = sku,
                ["maxSizeBytes"] = (long)maxSizeGb * 1024 * 1024 * 1024,
                ["collation"] = resource.Properties.GetValueOrDefault("collation", "SQL_Latin1_General_CP1_CI_AS"),
                ["zoneRedundant"] = resource.Properties.GetValueOrDefault("zoneRedundant", "false") == "true",
            }
        };
    }

    private const string SqlDatabaseTypesTemplate = """
        @export()
        @description('SKU name for the SQL Database')
        type SkuName = 'Basic' | 'Standard' | 'Premium' | 'GeneralPurpose' | 'BusinessCritical' | 'Hyperscale'
        """;

    private const string SqlDatabaseModuleTemplate = """
        import { SkuName } from './types.bicep'

        @description('Azure region for the SQL Database')
        param location string

        @description('Name of the SQL Database')
        param name string

        @description('Name of the parent SQL Server')
        param sqlServerName string

        @description('SKU of the SQL Database')
        param sku SkuName = 'Basic'

        @description('Maximum size of the database in bytes')
        param maxSizeBytes int

        @description('Collation of the database')
        param collation string

        @description('Whether the database is zone redundant')
        param zoneRedundant bool

        resource sqlServer 'Microsoft.Sql/servers@2023-08-01-preview' existing = {
          name: sqlServerName
        }

        resource sqlDatabase 'Microsoft.Sql/servers/databases@2023-08-01-preview' = {
          parent: sqlServer
          name: name
          location: location
          sku: {
            name: sku
          }
          properties: {
            collation: collation
            maxSizeBytes: maxSizeBytes
            zoneRedundant: zoneRedundant
          }
        }

        output id string = sqlDatabase.id
        """;
}
