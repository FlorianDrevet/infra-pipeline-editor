using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Builder;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.GenerationCore;

namespace InfraFlowSculptor.BicepGeneration.Generators;

/// <summary>Generates Bicep modules for Azure Function App resources with Code or Container deployment modes.</summary>
public sealed class FunctionAppTypeBicepGenerator
    : IResourceTypeBicepSpecGenerator
{
  private const string ManagedIdentityAcrAuthMode = "ManagedIdentity";
  private const string AdminCredentialsAcrAuthMode = "AdminCredentials";

    /// <inheritdoc />
    public string ResourceType
        => AzureResourceTypes.ArmTypes.FunctionApp;

    /// <inheritdoc />
    public string ResourceTypeName => AzureResourceTypes.FunctionApp;

    /// <inheritdoc />
    public BicepModuleSpec GenerateSpec(ResourceDefinition resource)
    {
        var deploymentMode = resource.Properties.GetValueOrDefault("deploymentMode", "Code");
        var isContainer = string.Equals(deploymentMode, "Container", StringComparison.OrdinalIgnoreCase);
        var acrAuthMode = GetAcrAuthMode(resource.Properties);
        var useAdminCredentials = isContainer
            && string.Equals(acrAuthMode, AdminCredentialsAcrAuthMode, StringComparison.OrdinalIgnoreCase);

        var builder = new BicepModuleBuilder()
            .Module("functionApp", "FunctionApp", ResourceTypeName)
            .Import("./types.bicep", "RuntimeStack", "WorkerRuntime")
            .Param("location", BicepType.String, "Azure region for the Function App")
            .Param("name", BicepType.String, "Name of the Function App")
            .Param("appServicePlanId", BicepType.String, "Resource ID of the App Service Plan")
            .Param("runtimeStack", BicepType.Custom("RuntimeStack"), "Runtime stack of the Function App",
                defaultValue: new BicepStringLiteral("DOTNET"))
            .Param("runtimeVersion", BicepType.String, "Runtime version (e.g. 8.0, 18)")
            .Param("httpsOnly", BicepType.Bool, "Whether HTTPS only is enforced")
            .Param("deploymentMode", BicepType.String, "Deployment mode",
                defaultValue: new BicepStringLiteral(isContainer ? "Container" : "Code"));

        // Container-specific params
        if (isContainer)
        {
            builder
                .Param("dockerImageName", BicepType.String, "Docker image name (e.g. myapp/functions)")
                .Param("dockerImageTag", BicepType.String, "Docker image tag (e.g. latest, v1.2.3)",
                    defaultValue: new BicepStringLiteral("latest"))
                .Param("acrLoginServer", BicepType.String, "ACR login server (e.g. myregistry.azurecr.io)");

            if (useAdminCredentials)
            {
                builder.Param("acrPassword", BicepType.String,
                    "Admin password for the Container Registry", secure: true);
            }
            else
            {
                builder
                    .Param("acrUseManagedIdentityCreds", BicepType.Bool,
                        "Whether to use managed identity credentials for ACR",
                        defaultValue: new BicepBoolLiteral(true))
                    .Param("acrUserManagedIdentityId", BicepType.String,
                        "Client ID of the user-assigned managed identity for ACR pull",
                        defaultValue: new BicepStringLiteral(""));
            }
        }

        builder.Param("customDomains", BicepType.Array, "Custom domain bindings for this Function App",
            defaultValue: new BicepArrayExpression([]));

        // Variables — workerRuntime is always present
        var workerRuntimeExpr = new BicepRawExpression(
            "toUpper(runtimeStack) == 'DOTNET' ? (contains(runtimeVersion, 'isolated') ? 'dotnet-isolated' : 'dotnet') : toLower(runtimeStack)");

        if (isContainer)
        {
            builder.Var("dockerImage", new BicepRawExpression("'${acrLoginServer}/${dockerImageName}:${dockerImageTag}'"));
            builder.Var("workerRuntime", workerRuntimeExpr);
            if (useAdminCredentials)
            {
                builder.Var("acrUsername", new BicepRawExpression("split(acrLoginServer, '.')[0]"));
            }

            builder.ModuleFileName(useAdminCredentials
                ? "functionAppContainerAdminCredentials"
                : "functionAppContainerManagedIdentity");
        }
        else
        {
            builder.Var("linuxFxVersion", new BicepRawExpression("'${toUpper(runtimeStack)}|${runtimeVersion}'"));
            builder.Var("workerRuntime", workerRuntimeExpr);
        }

        // Functions app settings (present in all variants)
        var functionsAppSettings = new List<BicepExpression>
        {
            new BicepObjectExpression([
                new BicepPropertyAssignment("name", new BicepStringLiteral("FUNCTIONS_WORKER_RUNTIME")),
                new BicepPropertyAssignment("value", new BicepReference("workerRuntime")),
            ]),
            new BicepObjectExpression([
                new BicepPropertyAssignment("name", new BicepStringLiteral("FUNCTIONS_EXTENSION_VERSION")),
                new BicepPropertyAssignment("value", new BicepStringLiteral("~4")),
            ]),
        };

        // Container Admin: add Docker registry settings
        if (isContainer && useAdminCredentials)
        {
            functionsAppSettings.Add(new BicepObjectExpression([
                new BicepPropertyAssignment("name", new BicepStringLiteral("DOCKER_REGISTRY_SERVER_URL")),
                new BicepPropertyAssignment("value", new BicepRawExpression("'https://${acrLoginServer}'")),
            ]));
            functionsAppSettings.Add(new BicepObjectExpression([
                new BicepPropertyAssignment("name", new BicepStringLiteral("DOCKER_REGISTRY_SERVER_USERNAME")),
                new BicepPropertyAssignment("value", new BicepReference("acrUsername")),
            ]));
            functionsAppSettings.Add(new BicepObjectExpression([
                new BicepPropertyAssignment("name", new BicepStringLiteral("DOCKER_REGISTRY_SERVER_PASSWORD")),
                new BicepPropertyAssignment("value", new BicepReference("acrPassword")),
            ]));
        }

        // Build siteConfig properties
        var siteConfigProps = new List<BicepPropertyAssignment>
        {
            new("linuxFxVersion", isContainer
                ? new BicepRawExpression("'DOCKER|${dockerImage}'")
                : new BicepReference("linuxFxVersion")),
            new("ftpsState", new BicepStringLiteral("Disabled")),
            new("minTlsVersion", new BicepStringLiteral("1.2")),
        };

        if (isContainer && !useAdminCredentials)
        {
            siteConfigProps.Add(new BicepPropertyAssignment("acrUseManagedIdentityCreds",
                new BicepReference("acrUseManagedIdentityCreds")));
            siteConfigProps.Add(new BicepPropertyAssignment("acrUserManagedIdentityID",
                new BicepConditionalExpression(
                    new BicepRawExpression("!empty(acrUserManagedIdentityId)"),
                    new BicepReference("acrUserManagedIdentityId"),
                    new BicepRawExpression("null"))));
        }
        else if (isContainer && useAdminCredentials)
        {
            siteConfigProps.Add(new BicepPropertyAssignment("acrUseManagedIdentityCreds",
                new BicepBoolLiteral(false)));
        }

        siteConfigProps.Add(new BicepPropertyAssignment("appSettings",
            new BicepArrayExpression(functionsAppSettings)));

        // Primary resource
        builder.Resource("functionApp", "Microsoft.Web/sites@2023-12-01")
            .Property("name", new BicepReference("name"))
            .Property("location", new BicepReference("location"))
            .Property("kind", new BicepStringLiteral(isContainer ? "functionapp,linux,container" : "functionapp"))
            .Property("properties", props => props
                .Property("serverFarmId", new BicepReference("appServicePlanId"))
                .Property("httpsOnly", new BicepReference("httpsOnly"))
                .Property("siteConfig", new BicepObjectExpression(siteConfigProps)));

        // hostNameBindings for-loop child resource
        builder.AdditionalResource("hostNameBindings", "Microsoft.Web/sites/hostNameBindings@2023-12-01",
            forLoop: new BicepForLoop("domain", new BicepReference("customDomains")),
            parentSymbol: "functionApp",
            bodyBuilder: body => body
                .Property("name", new BicepRawExpression("domain.domainName"))
                .Property("properties", p => p
                    .Property("siteName", new BicepRawExpression("functionApp.name"))
                    .Property("hostNameType", new BicepStringLiteral("Verified"))
                    .Property("sslState", new BicepConditionalExpression(
                        new BicepRawExpression("domain.bindingType == 'SniEnabled'"),
                        new BicepStringLiteral("SniEnabled"),
                        new BicepStringLiteral("Disabled")))));

        // Outputs
        builder
            .Output("id", BicepType.String, new BicepRawExpression("functionApp.id"),
                description: "The resource ID of the Function App")
            .Output("defaultHostName", BicepType.String,
                new BicepRawExpression("functionApp.properties.defaultHostName"),
                description: "The default host name of the Function App")
            .Output("principalId", BicepType.String,
                new BicepRawExpression("functionApp.identity.principalId"),
                description: "The principal ID of the system-assigned managed identity")
            .Output("customDomainVerificationId", BicepType.String,
                new BicepRawExpression("functionApp.properties.customDomainVerificationId"),
                description: "The custom domain verification ID");

        // Exported types
        builder
            .ExportedType("RuntimeStack",
                new BicepRawExpression("'DOTNET' | 'NODE' | 'PYTHON' | 'JAVA' | 'POWERSHELL'"),
                description: "Runtime stack for the Function App")
            .ExportedType("WorkerRuntime",
                new BicepRawExpression("'dotnet' | 'dotnet-isolated' | 'node' | 'python' | 'java' | 'powershell'"),
                description: "Functions worker runtime identifier")
            .ExportedType("DeploymentMode",
                new BicepRawExpression("'Code' | 'Container'"),
                description: "Deployment mode for the Function App");

        return builder.Build();
    }

      private static string GetAcrAuthMode(IReadOnlyDictionary<string, string> properties)
      {
        var acrAuthMode = properties.GetValueOrDefault("acrAuthMode", string.Empty);
        return string.IsNullOrWhiteSpace(acrAuthMode)
          ? ManagedIdentityAcrAuthMode
          : acrAuthMode;
      }

    /// <inheritdoc />
    public GeneratedTypeModule Generate(ResourceDefinition resource)
    {
        var deploymentMode = resource.Properties.GetValueOrDefault("deploymentMode", "Code");
        var isContainer = string.Equals(deploymentMode, "Container", StringComparison.OrdinalIgnoreCase);
        var acrAuthMode = GetAcrAuthMode(resource.Properties);
        var useAdminCredentials = isContainer
            && string.Equals(acrAuthMode, AdminCredentialsAcrAuthMode, StringComparison.OrdinalIgnoreCase);

        var moduleFileName = isContainer
            ? useAdminCredentials
                ? "functionAppContainerAdminCredentials"
                : "functionAppContainerManagedIdentity"
            : "functionApp";

        return new GeneratedTypeModule
        {
            ModuleName = "functionApp",
            ModuleFileName = moduleFileName,
            ModuleFolderName = "FunctionApp",
            ModuleBicepContent = isContainer
                ? useAdminCredentials
                    ? FunctionAppContainerAdminCredentialsModuleTemplate
                    : FunctionAppContainerManagedIdentityModuleTemplate
                : FunctionAppCodeModuleTemplate,
            ModuleTypesBicepContent = FunctionAppTypesTemplate,
            ResourceTypeName = ResourceTypeName,
            SecureParameters = isContainer && useAdminCredentials ? ["acrPassword"] : [],
        };
    }

    private const string FunctionAppTypesTemplate = """
        @export()
        @description('Runtime stack for the Function App')
        type RuntimeStack = 'DOTNET' | 'NODE' | 'PYTHON' | 'JAVA' | 'POWERSHELL'

        @export()
        @description('Functions worker runtime identifier')
        type WorkerRuntime = 'dotnet' | 'dotnet-isolated' | 'node' | 'python' | 'java' | 'powershell'

        @export()
        @description('Deployment mode for the Function App')
        type DeploymentMode = 'Code' | 'Container'
        """;

    private const string FunctionAppCodeModuleTemplate = """
        import { RuntimeStack, WorkerRuntime } from './types.bicep'

        @description('Azure region for the Function App')
        param location string

        @description('Name of the Function App')
        param name string

        @description('Resource ID of the App Service Plan')
        param appServicePlanId string

        @description('Runtime stack of the Function App')
        param runtimeStack RuntimeStack = 'DOTNET'

        @description('Runtime version (e.g. 8.0, 18)')
        param runtimeVersion string

        @description('Whether HTTPS only is enforced')
        param httpsOnly bool

        @description('Deployment mode')
        param deploymentMode string = 'Code'

        @description('Custom domain bindings for this Function App')
        param customDomains array = []

        var linuxFxVersion = '${toUpper(runtimeStack)}|${runtimeVersion}'
        var workerRuntime = toUpper(runtimeStack) == 'DOTNET'
          ? (contains(runtimeVersion, 'isolated') ? 'dotnet-isolated' : 'dotnet')
          : toLower(runtimeStack)

        resource functionApp 'Microsoft.Web/sites@2023-12-01' = {
          name: name
          location: location
          kind: 'functionapp'
          properties: {
            serverFarmId: appServicePlanId
            httpsOnly: httpsOnly
            siteConfig: {
              linuxFxVersion: linuxFxVersion
              ftpsState: 'Disabled'
              minTlsVersion: '1.2'
              appSettings: [
                {
                  name: 'FUNCTIONS_WORKER_RUNTIME'
                  value: workerRuntime
                }
                {
                  name: 'FUNCTIONS_EXTENSION_VERSION'
                  value: '~4'
                }
              ]
            }
          }
        }

        resource hostNameBindings 'Microsoft.Web/sites/hostNameBindings@2023-12-01' = [for domain in customDomains: {
          parent: functionApp
          name: domain.domainName
          properties: {
            siteName: functionApp.name
            hostNameType: 'Verified'
            sslState: domain.bindingType == 'SniEnabled' ? 'SniEnabled' : 'Disabled'
          }
        }]

        @description('The resource ID of the Function App')
        output id string = functionApp.id

        @description('The default host name of the Function App')
        output defaultHostName string = functionApp.properties.defaultHostName

        @description('The principal ID of the system-assigned managed identity')
        output principalId string = functionApp.identity.principalId

        @description('The custom domain verification ID')
        output customDomainVerificationId string = functionApp.properties.customDomainVerificationId
        """;

    private const string FunctionAppContainerManagedIdentityModuleTemplate = """
        import { RuntimeStack, WorkerRuntime } from './types.bicep'

        @description('Azure region for the Function App')
        param location string

        @description('Name of the Function App')
        param name string

        @description('Resource ID of the App Service Plan')
        param appServicePlanId string

        @description('Runtime stack of the Function App')
        param runtimeStack RuntimeStack = 'DOTNET'

        @description('Runtime version (e.g. 8.0, 18)')
        param runtimeVersion string

        @description('Whether HTTPS only is enforced')
        param httpsOnly bool

        @description('Deployment mode')
        param deploymentMode string = 'Container'

        @description('Docker image name (e.g. myapp/functions)')
        param dockerImageName string

        @description('Docker image tag (e.g. latest, v1.2.3)')
        param dockerImageTag string = 'latest'

        @description('ACR login server (e.g. myregistry.azurecr.io)')
        param acrLoginServer string

        @description('Whether to use managed identity credentials for ACR')
        param acrUseManagedIdentityCreds bool = true

        @description('Client ID of the user-assigned managed identity for ACR pull')
        param acrUserManagedIdentityId string = ''

        @description('Custom domain bindings for this Function App')
        param customDomains array = []

        var dockerImage = '${acrLoginServer}/${dockerImageName}:${dockerImageTag}'
        var workerRuntime = toUpper(runtimeStack) == 'DOTNET'
          ? (contains(runtimeVersion, 'isolated') ? 'dotnet-isolated' : 'dotnet')
          : toLower(runtimeStack)

        resource functionApp 'Microsoft.Web/sites@2023-12-01' = {
          name: name
          location: location
          kind: 'functionapp,linux,container'
          properties: {
            serverFarmId: appServicePlanId
            httpsOnly: httpsOnly
            siteConfig: {
              linuxFxVersion: 'DOCKER|${dockerImage}'
              ftpsState: 'Disabled'
              minTlsVersion: '1.2'
              acrUseManagedIdentityCreds: acrUseManagedIdentityCreds
              acrUserManagedIdentityID: !empty(acrUserManagedIdentityId) ? acrUserManagedIdentityId : null
              appSettings: [
                {
                  name: 'FUNCTIONS_WORKER_RUNTIME'
                  value: workerRuntime
                }
                {
                  name: 'FUNCTIONS_EXTENSION_VERSION'
                  value: '~4'
                }
              ]
            }
          }
        }

        resource hostNameBindings 'Microsoft.Web/sites/hostNameBindings@2023-12-01' = [for domain in customDomains: {
          parent: functionApp
          name: domain.domainName
          properties: {
            siteName: functionApp.name
            hostNameType: 'Verified'
            sslState: domain.bindingType == 'SniEnabled' ? 'SniEnabled' : 'Disabled'
          }
        }]

        @description('The resource ID of the Function App')
        output id string = functionApp.id

        @description('The default host name of the Function App')
        output defaultHostName string = functionApp.properties.defaultHostName

        @description('The principal ID of the system-assigned managed identity')
        output principalId string = functionApp.identity.principalId

        @description('The custom domain verification ID')
        output customDomainVerificationId string = functionApp.properties.customDomainVerificationId
        """;

    private const string FunctionAppContainerAdminCredentialsModuleTemplate = """
        import { RuntimeStack, WorkerRuntime } from './types.bicep'

        @description('Azure region for the Function App')
        param location string

        @description('Name of the Function App')
        param name string

        @description('Resource ID of the App Service Plan')
        param appServicePlanId string

        @description('Runtime stack of the Function App')
        param runtimeStack RuntimeStack = 'DOTNET'

        @description('Runtime version (e.g. 8.0, 18)')
        param runtimeVersion string

        @description('Whether HTTPS only is enforced')
        param httpsOnly bool

        @description('Deployment mode')
        param deploymentMode string = 'Container'

        @description('Docker image name (e.g. myapp/functions)')
        param dockerImageName string

        @description('Docker image tag (e.g. latest, v1.2.3)')
        param dockerImageTag string = 'latest'

        @description('ACR login server (e.g. myregistry.azurecr.io)')
        param acrLoginServer string

        @secure()
        @description('Admin password for the Container Registry')
        param acrPassword string

        @description('Custom domain bindings for this Function App')
        param customDomains array = []

        var dockerImage = '${acrLoginServer}/${dockerImageName}:${dockerImageTag}'
        var workerRuntime = toUpper(runtimeStack) == 'DOTNET'
          ? (contains(runtimeVersion, 'isolated') ? 'dotnet-isolated' : 'dotnet')
          : toLower(runtimeStack)
        var acrUsername = split(acrLoginServer, '.')[0]

        resource functionApp 'Microsoft.Web/sites@2023-12-01' = {
          name: name
          location: location
          kind: 'functionapp,linux,container'
          properties: {
            serverFarmId: appServicePlanId
            httpsOnly: httpsOnly
            siteConfig: {
              linuxFxVersion: 'DOCKER|${dockerImage}'
              ftpsState: 'Disabled'
              minTlsVersion: '1.2'
              acrUseManagedIdentityCreds: false
              appSettings: [
                {
                  name: 'FUNCTIONS_WORKER_RUNTIME'
                  value: workerRuntime
                }
                {
                  name: 'FUNCTIONS_EXTENSION_VERSION'
                  value: '~4'
                }
                {
                  name: 'DOCKER_REGISTRY_SERVER_URL'
                  value: 'https://${acrLoginServer}'
                }
                {
                  name: 'DOCKER_REGISTRY_SERVER_USERNAME'
                  value: acrUsername
                }
                {
                  name: 'DOCKER_REGISTRY_SERVER_PASSWORD'
                  value: acrPassword
                }
              ]
            }
          }
        }

        resource hostNameBindings 'Microsoft.Web/sites/hostNameBindings@2023-12-01' = [for domain in customDomains: {
          parent: functionApp
          name: domain.domainName
          properties: {
            siteName: functionApp.name
            hostNameType: 'Verified'
            sslState: domain.bindingType == 'SniEnabled' ? 'SniEnabled' : 'Disabled'
          }
        }]

        @description('The resource ID of the Function App')
        output id string = functionApp.id

        @description('The default host name of the Function App')
        output defaultHostName string = functionApp.properties.defaultHostName

        @description('The principal ID of the system-assigned managed identity')
        output principalId string = functionApp.identity.principalId

        @description('The custom domain verification ID')
        output customDomainVerificationId string = functionApp.properties.customDomainVerificationId
        """;
}
