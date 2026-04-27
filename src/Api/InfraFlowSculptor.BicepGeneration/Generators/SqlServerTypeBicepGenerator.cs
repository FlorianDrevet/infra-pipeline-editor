using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Builder;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.GenerationCore;

namespace InfraFlowSculptor.BicepGeneration.Generators;

/// <summary>Generates Bicep module for Azure SQL Server (<c>Microsoft.Sql/servers@2023-08-01-preview</c>).</summary>
public sealed class SqlServerTypeBicepGenerator
    : IResourceTypeBicepSpecGenerator
{
    /// <inheritdoc />
    public string ResourceType
        => AzureResourceTypes.ArmTypes.SqlServer;

    /// <inheritdoc />
    public string ResourceTypeName => AzureResourceTypes.SqlServer;

    /// <inheritdoc />
    public BicepModuleSpec GenerateSpec(ResourceDefinition resource)
    {
        return new BicepModuleBuilder()
            .Module("sqlServer", "SqlServer", ResourceTypeName)
            .Import("./types.bicep", "SqlServerVersion", "TlsVersion")
            .Param("location", BicepType.String, "Azure region for the SQL Server")
            .Param("name", BicepType.String, "Name of the SQL Server")
            .Param("version", BicepType.Custom("SqlServerVersion"), "SQL Server version",
                defaultValue: new BicepStringLiteral("12.0"))
            .Param("administratorLogin", BicepType.String, "Administrator login name")
            .Param("administratorLoginPassword", BicepType.String, "Administrator login password",
                secure: true)
            .Param("minimalTlsVersion", BicepType.Custom("TlsVersion"), "Minimum TLS version for client connections",
                defaultValue: new BicepStringLiteral("1.2"))
            .Resource("sqlServer", "Microsoft.Sql/servers@2023-08-01-preview")
            .Property("name", new BicepReference("name"))
            .Property("location", new BicepReference("location"))
            .Property("properties", props => props
                .Property("version", new BicepReference("version"))
                .Property("administratorLogin", new BicepReference("administratorLogin"))
                .Property("administratorLoginPassword", new BicepReference("administratorLoginPassword"))
                .Property("minimalTlsVersion", new BicepReference("minimalTlsVersion"))
                .Property("publicNetworkAccess", new BicepStringLiteral("Enabled")))
            .Output("id", BicepType.String, new BicepRawExpression("sqlServer.id"),
                description: "The resource ID of the SQL Server")
            .Output("fullyQualifiedDomainName", BicepType.String,
                new BicepRawExpression("sqlServer.properties.fullyQualifiedDomainName"),
                description: "The fully qualified domain name of the SQL Server")
            .ExportedType("SqlServerVersion",
                new BicepRawExpression("'12.0'"),
                description: "SQL Server version")
            .ExportedType("TlsVersion",
                new BicepRawExpression("'1.0' | '1.1' | '1.2'"),
                description: "Minimum TLS version for SQL Server connections")
            .Build();
    }

    /// <inheritdoc />
    public GeneratedTypeModule Generate(ResourceDefinition resource)
    {
        var rawVersion = resource.Properties.GetValueOrDefault("version", "12.0");
        var version = NormalizeSqlServerVersion(rawVersion);

        return new GeneratedTypeModule
        {
            ModuleName = "sqlServer",
            ModuleFileName = "sqlServer",
            ModuleFolderName = "SqlServer",
            ModuleBicepContent = SqlServerModuleTemplate,
            ModuleTypesBicepContent = SqlServerTypesTemplate,
            ResourceTypeName = ResourceTypeName,
            Parameters = new Dictionary<string, object>
            {
                ["version"] = version,
                ["administratorLogin"] = resource.Properties.GetValueOrDefault("administratorLogin", "sqladmin"),
                ["minimalTlsVersion"] = resource.Properties.GetValueOrDefault("minimalTlsVersion", "1.2"),
            },
            SecureParameters = ["administratorLoginPassword"]
        };
    }

    /// <summary>
    /// Normalizes legacy SQL Server version identifiers (e.g. <c>V12</c>) to the ARM-accepted format (<c>12.0</c>).
    /// </summary>
    private static string NormalizeSqlServerVersion(string version) => version.ToUpperInvariant() switch
    {
        "V12" => "12.0",
        _ => version
    };

    private const string SqlServerTypesTemplate = """
        @export()
        @description('SQL Server version')
        type SqlServerVersion = '12.0'

        @export()
        @description('Minimum TLS version for SQL Server connections')
        type TlsVersion = '1.0' | '1.1' | '1.2'
        """;

    private const string SqlServerModuleTemplate = """
        import { SqlServerVersion, TlsVersion } from './types.bicep'

        @description('Azure region for the SQL Server')
        param location string

        @description('Name of the SQL Server')
        param name string

        @description('SQL Server version')
        param version SqlServerVersion = '12.0'

        @description('Administrator login name')
        param administratorLogin string

        @secure()
        @description('Administrator login password')
        param administratorLoginPassword string

        @description('Minimum TLS version for client connections')
        param minimalTlsVersion TlsVersion = '1.2'

        resource sqlServer 'Microsoft.Sql/servers@2023-08-01-preview' = {
          name: name
          location: location
          properties: {
            version: version
            administratorLogin: administratorLogin
            administratorLoginPassword: administratorLoginPassword
            minimalTlsVersion: minimalTlsVersion
            publicNetworkAccess: 'Enabled'
          }
        }

        output id string = sqlServer.id
        output fullyQualifiedDomainName string = sqlServer.properties.fullyQualifiedDomainName
        """;
}
