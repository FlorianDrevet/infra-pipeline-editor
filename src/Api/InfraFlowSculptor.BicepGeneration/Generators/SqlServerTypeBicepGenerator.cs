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
            ModuleBicepContent = SqlServerModuleTemplate,
            ResourceTypeName = ResourceTypeName,
            Parameters = new Dictionary<string, object>
            {
                ["version"] = resource.Properties.GetValueOrDefault("version", "12.0"),
                ["administratorLogin"] = resource.Properties.GetValueOrDefault("administratorLogin", "sqladmin"),
                ["minimalTlsVersion"] = resource.Properties.GetValueOrDefault("minimalTlsVersion", "1.2"),
            }
        };
    }

    private const string SqlServerModuleTemplate = """
        param location string
        param name string
        param version string
        param administratorLogin string
        @secure()
        param administratorLoginPassword string
        param minimalTlsVersion string

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
