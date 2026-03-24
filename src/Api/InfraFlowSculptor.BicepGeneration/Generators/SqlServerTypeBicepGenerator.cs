using InfraFlowSculptor.BicepGeneration.Models;

namespace InfraFlowSculptor.BicepGeneration.Generators;

/// <summary>Generates Bicep module for Azure SQL Server (<c>Microsoft.Sql/servers@2023-08-01-preview</c>).</summary>
public sealed class SqlServerTypeBicepGenerator
    : IResourceTypeBicepGenerator
{
    /// <inheritdoc />
    public string ResourceType
        => "Microsoft.Sql/servers";

    /// <inheritdoc />
    public string ResourceTypeName => "SqlServer";

    /// <inheritdoc />
    public GeneratedTypeModule Generate(ResourceDefinition resource)
    {
        return new GeneratedTypeModule
        {
            ModuleName = "sqlServer",
            ModuleFileName = "sqlServer.bicep",
            ModuleFolderName = "SqlServer",
            ModuleBicepContent = SqlServerModuleTemplate,
            ModuleTypesBicepContent = SqlServerTypesTemplate,
            ResourceTypeName = ResourceTypeName,
            Parameters = new Dictionary<string, object>
            {
                ["version"] = resource.Properties.GetValueOrDefault("version", "12.0"),
                ["administratorLogin"] = resource.Properties.GetValueOrDefault("administratorLogin", "sqladmin"),
                ["minimalTlsVersion"] = resource.Properties.GetValueOrDefault("minimalTlsVersion", "1.2"),
            }
        };
    }

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
