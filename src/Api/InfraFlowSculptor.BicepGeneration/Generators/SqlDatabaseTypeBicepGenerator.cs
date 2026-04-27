using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Builder;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.GenerationCore;

namespace InfraFlowSculptor.BicepGeneration.Generators;

/// <summary>Generates Bicep module for Azure SQL Database (<c>Microsoft.Sql/servers/databases@2023-08-01-preview</c>).</summary>
public sealed class SqlDatabaseTypeBicepGenerator
    : IResourceTypeBicepSpecGenerator
{
    /// <inheritdoc />
    public string ResourceType
        => AzureResourceTypes.ArmTypes.SqlDatabase;

    /// <inheritdoc />
    public string ResourceTypeName => AzureResourceTypes.SqlDatabase;

    /// <inheritdoc />
    public BicepModuleSpec GenerateSpec(ResourceDefinition resource)
    {
        return new BicepModuleBuilder()
            .Module("sqlDatabase", "SqlDatabase", ResourceTypeName)
            .Import("./types.bicep", "SkuName")
            .Param("location", BicepType.String, "Azure region for the SQL Database")
            .Param("name", BicepType.String, "Name of the SQL Database")
            .Param("sqlServerName", BicepType.String, "Name of the parent SQL Server")
            .Param("sku", BicepType.Custom("SkuName"), "SKU of the SQL Database",
                defaultValue: new BicepStringLiteral("Basic"))
            .Param("maxSizeBytes", BicepType.Int, "Maximum size of the database in bytes")
            .Param("collation", BicepType.String, "Collation of the database")
            .Param("zoneRedundant", BicepType.Bool, "Whether the database is zone redundant")
            .ExistingResource("sqlServer", "Microsoft.Sql/servers@2023-08-01-preview", "sqlServerName")
            .Resource("sqlDatabase", "Microsoft.Sql/servers/databases@2023-08-01-preview")
            .Parent("sqlServer")
            .Property("name", new BicepReference("name"))
            .Property("location", new BicepReference("location"))
            .Property("sku", sku => sku
                .Property("name", new BicepReference("sku")))
            .Property("properties", props => props
                .Property("collation", new BicepReference("collation"))
                .Property("maxSizeBytes", new BicepReference("maxSizeBytes"))
                .Property("zoneRedundant", new BicepReference("zoneRedundant")))
            .Output("id", BicepType.String, new BicepRawExpression("sqlDatabase.id"),
                description: "The resource ID of the SQL Database")
            .ExportedType("SkuName",
                new BicepRawExpression("'Basic' | 'Standard' | 'Premium' | 'GeneralPurpose' | 'BusinessCritical' | 'Hyperscale'"),
                description: "SKU name for the SQL Database")
            .Build();
    }

    /// <inheritdoc />
    public GeneratedTypeModule Generate(ResourceDefinition resource)
    {
        var sku = resource.Properties.GetValueOrDefault("sku", "Basic");
        var maxSizeGb = int.TryParse(resource.Properties.GetValueOrDefault("maxSizeGb", "2"), out var sz) ? sz : 2;

        return new GeneratedTypeModule
        {
            ModuleName = "sqlDatabase",
            ModuleFileName = "sqlDatabase",
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
