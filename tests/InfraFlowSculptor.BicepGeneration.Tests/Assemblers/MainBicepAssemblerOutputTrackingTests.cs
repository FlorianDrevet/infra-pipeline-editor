using FluentAssertions;
using InfraFlowSculptor.BicepGeneration.Assemblers;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.BicepGeneration.Tests.Assemblers;

/// <summary>
/// Unit tests for <see cref="MainBicepAssembler"/> output usage tracking.
/// Validates that every <c>{symbol}Module.outputs.{outputName}</c> reference emitted
/// in <c>main.bicep</c> is registered in <see cref="MainBicepEmissionResult.UsedOutputsByModulePath"/>,
/// keyed by the corresponding module file path (case-insensitive).
/// </summary>
public sealed class MainBicepAssemblerOutputTrackingTests
{
    private const string AppServicePlanFolder = "AppServicePlan";
    private const string AppServicePlanFile = "appServicePlan.module.bicep";
    private const string WebAppFolder = "WebApp";
    private const string WebAppFile = "webApp.module.bicep";
    private const string KeyVaultFolder = "KeyVault";
    private const string KeyVaultFile = "keyVault.module.bicep";
    private const string KvSecretsPath = "modules/KeyVault/kvSecrets.module.bicep";
    private const string UaiFolder = "UserAssignedIdentity";
    private const string UaiFile = "userAssignedIdentity.module.bicep";
    private const string ContainerAppFolder = "ContainerApp";
    private const string ContainerAppFile = "containerApp.module.bicep";

    [Fact]
    public void Generate_TracksParentIdReference_ForChildResource()
    {
        // Arrange — webApp depends on appServicePlan via ParentModuleIdReferences
        var asp = NewModule(
            moduleName: "appServicePlanMyPlan",
            logicalName: "myPlan",
            resourceTypeName: AzureResourceTypes.AppServicePlan,
            folder: AppServicePlanFolder,
            file: AppServicePlanFile,
            resourceGroup: "rg-app");

        var web = NewModule(
            moduleName: "webAppMyApp",
            logicalName: "myApp",
            resourceTypeName: AzureResourceTypes.WebApp,
            folder: WebAppFolder,
            file: WebAppFile,
            resourceGroup: "rg-app",
            parentIdReferences: new Dictionary<string, (string Name, string ResourceTypeName)>
            {
                ["appServicePlanId"] = ("myPlan", AzureResourceTypes.AppServicePlan),
            });

        var rg = NewResourceGroup("rg-app");

        // Act
        var result = MainBicepAssembler.Generate(
            modules: [asp, web],
            resourceGroups: [rg],
            namingContext: new NamingContext(),
            roleAssignments: [],
            appSettings: [],
            existingResourceReferences: []);

        // Assert
        var aspPath = $"modules/{AppServicePlanFolder}/{AppServicePlanFile}";
        result.UsedOutputsByModulePath.Should().ContainKey(aspPath);
        result.UsedOutputsByModulePath[aspPath].Should().Contain("id");
    }

    [Fact]
    public void Generate_TracksResourceId_ForUserAssignedIdentityReference()
    {
        // Arrange — webApp uses a User-Assigned Identity (uai)
        var uai = NewModule(
            moduleName: "userAssignedIdentityMyUai",
            logicalName: "myUai",
            resourceTypeName: "UserAssignedIdentity",
            folder: UaiFolder,
            file: UaiFile,
            resourceGroup: "rg-app");

        var web = NewModule(
            moduleName: "webAppMyApp",
            logicalName: "myApp",
            resourceTypeName: AzureResourceTypes.WebApp,
            folder: WebAppFolder,
            file: WebAppFile,
            resourceGroup: "rg-app");

        var roleAssignment = new RoleAssignmentDefinition
        {
            SourceResourceName = "myApp",
            SourceResourceType = "Microsoft.Web/sites",
            SourceResourceTypeName = AzureResourceTypes.WebApp,
            SourceResourceGroupName = "rg-app",
            TargetResourceName = "myKv",
            TargetResourceType = "Microsoft.KeyVault/vaults",
            TargetResourceTypeName = AzureResourceTypes.KeyVault,
            TargetResourceGroupName = "rg-app",
            TargetResourceAbbreviation = "kv",
            ManagedIdentityType = "UserAssigned",
            UserAssignedIdentityName = "myUai",
            UserAssignedIdentityResourceId = Guid.NewGuid(),
            RoleDefinitionId = "00000000-0000-0000-0000-000000000001",
            RoleDefinitionName = "Key Vault Secrets User",
            ServiceCategory = "keyvault",
        };

        var rg = NewResourceGroup("rg-app");

        // Act
        var result = MainBicepAssembler.Generate(
            modules: [uai, web],
            resourceGroups: [rg],
            namingContext: new NamingContext(),
            roleAssignments: [roleAssignment],
            appSettings: [],
            existingResourceReferences: []);

        // Assert
        var uaiPath = $"modules/{UaiFolder}/{UaiFile}";
        result.UsedOutputsByModulePath.Should().ContainKey(uaiPath);
        result.UsedOutputsByModulePath[uaiPath].Should().Contain("resourceId");
    }

    [Fact]
    public void Generate_TracksVaultUri_ForKeyVaultReference()
    {
        // Arrange — webApp pulls a Key Vault secret via ViaKeyVaultDirect (DirectInKeyVault)
        var kv = NewModule(
            moduleName: "keyVaultMyKv",
            logicalName: "myKv",
            resourceTypeName: AzureResourceTypes.KeyVault,
            folder: KeyVaultFolder,
            file: KeyVaultFile,
            resourceGroup: "rg-app");

        var web = NewModule(
            moduleName: "webAppMyApp",
            logicalName: "myApp",
            resourceTypeName: AzureResourceTypes.WebApp,
            folder: WebAppFolder,
            file: WebAppFile,
            resourceGroup: "rg-app");

        var setting = new AppSettingDefinition
        {
            Name = "DbPassword",
            TargetResourceName = "myApp",
            IsKeyVaultReference = true,
            KeyVaultResourceName = "myKv",
            SecretName = "db-password",
            SecretValueAssignment = "DirectInKeyVault",
        };

        var rg = NewResourceGroup("rg-app");

        // Act
        var result = MainBicepAssembler.Generate(
            modules: [kv, web],
            resourceGroups: [rg],
            namingContext: new NamingContext(),
            roleAssignments: [],
            appSettings: [setting],
            existingResourceReferences: []);

        // Assert
        var kvPath = $"modules/{KeyVaultFolder}/{KeyVaultFile}";
        result.UsedOutputsByModulePath.Should().ContainKey(kvPath);
        result.UsedOutputsByModulePath[kvPath].Should().Contain("vaultUri");
    }

    [Fact]
    public void Generate_TracksSecretUris_ForKeyVaultSecretsModule()
    {
        // Arrange — webApp pulls a Key Vault secret via ViaBicepparam, which spawns a kvSecretsModule
        var kv = NewModule(
            moduleName: "keyVaultMyKv",
            logicalName: "myKv",
            resourceTypeName: AzureResourceTypes.KeyVault,
            folder: KeyVaultFolder,
            file: KeyVaultFile,
            resourceGroup: "rg-app");

        var web = NewModule(
            moduleName: "webAppMyApp",
            logicalName: "myApp",
            resourceTypeName: AzureResourceTypes.WebApp,
            folder: WebAppFolder,
            file: WebAppFile,
            resourceGroup: "rg-app");

        var setting = new AppSettingDefinition
        {
            Name = "DbPassword",
            TargetResourceName = "myApp",
            IsKeyVaultReference = true,
            KeyVaultResourceName = "myKv",
            SecretName = "db-password",
            SecretValueAssignment = "ViaBicepparam",
        };

        var rg = NewResourceGroup("rg-app");

        // Act
        var result = MainBicepAssembler.Generate(
            modules: [kv, web],
            resourceGroups: [rg],
            namingContext: new NamingContext(),
            roleAssignments: [],
            appSettings: [setting],
            existingResourceReferences: []);

        // Assert — the kvSecretsModule is emitted at modules/KeyVault/kvSecrets.module.bicep
        // and the value expression references its 'secretUris' output.
        result.UsedOutputsByModulePath.Should().ContainKey(KvSecretsPath);
        result.UsedOutputsByModulePath[KvSecretsPath].Should().Contain("secretUris");
    }

    [Fact]
    public void Generate_TracksAppSettingOutputReference()
    {
        // Arrange — webApp pulls an output from another module ("storageMyStorage.outputs.connectionString")
        var storage = NewModule(
            moduleName: "storageAccountMyStorage",
            logicalName: "myStorage",
            resourceTypeName: AzureResourceTypes.StorageAccount,
            folder: "StorageAccount",
            file: "storageAccount.module.bicep",
            resourceGroup: "rg-app");

        var web = NewModule(
            moduleName: "webAppMyApp",
            logicalName: "myApp",
            resourceTypeName: AzureResourceTypes.WebApp,
            folder: WebAppFolder,
            file: WebAppFile,
            resourceGroup: "rg-app");

        var setting = new AppSettingDefinition
        {
            Name = "StorageConnection",
            TargetResourceName = "myApp",
            IsOutputReference = true,
            SourceResourceName = "myStorage",
            SourceResourceTypeName = AzureResourceTypes.StorageAccount,
            SourceOutputName = "connectionString",
            SourceOutputBicepExpression = "storage.properties.primaryEndpoints.blob",
        };

        var rg = NewResourceGroup("rg-app");

        // Act
        var result = MainBicepAssembler.Generate(
            modules: [storage, web],
            resourceGroups: [rg],
            namingContext: new NamingContext(),
            roleAssignments: [],
            appSettings: [setting],
            existingResourceReferences: []);

        // Assert
        const string storagePath = "modules/StorageAccount/storageAccount.module.bicep";
        result.UsedOutputsByModulePath.Should().ContainKey(storagePath);
        result.UsedOutputsByModulePath[storagePath].Should().Contain("connectionString");
    }

    [Fact]
    public void Generate_TracksPrincipalId_ForSystemAssignedRoleAssignment()
    {
        // Arrange — containerApp (system-assigned identity) gets RBAC on a key vault
        var kv = NewModule(
            moduleName: "keyVaultMyKv",
            logicalName: "myKv",
            resourceTypeName: AzureResourceTypes.KeyVault,
            folder: KeyVaultFolder,
            file: KeyVaultFile,
            resourceGroup: "rg-app");

        var ca = NewModule(
            moduleName: "containerAppMyApp",
            logicalName: "myApp",
            resourceTypeName: AzureResourceTypes.ContainerApp,
            folder: ContainerAppFolder,
            file: ContainerAppFile,
            resourceGroup: "rg-app");

        var roleAssignment = new RoleAssignmentDefinition
        {
            SourceResourceName = "myApp",
            SourceResourceType = "Microsoft.App/containerApps",
            SourceResourceTypeName = AzureResourceTypes.ContainerApp,
            SourceResourceGroupName = "rg-app",
            TargetResourceName = "myKv",
            TargetResourceType = "Microsoft.KeyVault/vaults",
            TargetResourceTypeName = AzureResourceTypes.KeyVault,
            TargetResourceGroupName = "rg-app",
            TargetResourceAbbreviation = "kv",
            ManagedIdentityType = "SystemAssigned",
            RoleDefinitionId = "00000000-0000-0000-0000-000000000002",
            RoleDefinitionName = "Key Vault Secrets User",
            ServiceCategory = "keyvault",
        };

        var rg = NewResourceGroup("rg-app");

        // Act
        var result = MainBicepAssembler.Generate(
            modules: [kv, ca],
            resourceGroups: [rg],
            namingContext: new NamingContext(),
            roleAssignments: [roleAssignment],
            appSettings: [],
            existingResourceReferences: []);

        // Assert — the containerApp module exposes the principalId output that the role assignment consumes.
        var caPath = $"modules/{ContainerAppFolder}/{ContainerAppFile}";
        result.UsedOutputsByModulePath.Should().ContainKey(caPath);
        result.UsedOutputsByModulePath[caPath].Should().Contain("principalId");
    }

    [Fact]
    public void Generate_TracksPrincipalId_ForUserAssignedRoleAssignment()
    {
        // Arrange — same scenario but using a UAI as identity provider
        var uai = NewModule(
            moduleName: "userAssignedIdentityMyUai",
            logicalName: "myUai",
            resourceTypeName: "UserAssignedIdentity",
            folder: UaiFolder,
            file: UaiFile,
            resourceGroup: "rg-app");

        var kv = NewModule(
            moduleName: "keyVaultMyKv",
            logicalName: "myKv",
            resourceTypeName: AzureResourceTypes.KeyVault,
            folder: KeyVaultFolder,
            file: KeyVaultFile,
            resourceGroup: "rg-app");

        var web = NewModule(
            moduleName: "webAppMyApp",
            logicalName: "myApp",
            resourceTypeName: AzureResourceTypes.WebApp,
            folder: WebAppFolder,
            file: WebAppFile,
            resourceGroup: "rg-app");

        var roleAssignment = new RoleAssignmentDefinition
        {
            SourceResourceName = "myApp",
            SourceResourceType = "Microsoft.Web/sites",
            SourceResourceTypeName = AzureResourceTypes.WebApp,
            SourceResourceGroupName = "rg-app",
            TargetResourceName = "myKv",
            TargetResourceType = "Microsoft.KeyVault/vaults",
            TargetResourceTypeName = AzureResourceTypes.KeyVault,
            TargetResourceGroupName = "rg-app",
            TargetResourceAbbreviation = "kv",
            ManagedIdentityType = "UserAssigned",
            UserAssignedIdentityName = "myUai",
            UserAssignedIdentityResourceId = Guid.NewGuid(),
            RoleDefinitionId = "00000000-0000-0000-0000-000000000003",
            RoleDefinitionName = "Key Vault Secrets User",
            ServiceCategory = "keyvault",
        };

        var rg = NewResourceGroup("rg-app");

        // Act
        var result = MainBicepAssembler.Generate(
            modules: [uai, kv, web],
            resourceGroups: [rg],
            namingContext: new NamingContext(),
            roleAssignments: [roleAssignment],
            appSettings: [],
            existingResourceReferences: []);

        // Assert — the UAI module exposes the principalId output that the role assignment consumes.
        var uaiPath = $"modules/{UaiFolder}/{UaiFile}";
        result.UsedOutputsByModulePath.Should().ContainKey(uaiPath);
        result.UsedOutputsByModulePath[uaiPath].Should().Contain("principalId");
    }

    [Fact]
    public void Generate_DoesNotTrack_WhenNoReferencesEmitted()
    {
        // Arrange — a single isolated module with no parent / role / app-setting references.
        var kv = NewModule(
            moduleName: "keyVaultMyKv",
            logicalName: "myKv",
            resourceTypeName: AzureResourceTypes.KeyVault,
            folder: KeyVaultFolder,
            file: KeyVaultFile,
            resourceGroup: "rg-app");

        var rg = NewResourceGroup("rg-app");

        // Act
        var result = MainBicepAssembler.Generate(
            modules: [kv],
            resourceGroups: [rg],
            namingContext: new NamingContext(),
            roleAssignments: [],
            appSettings: [],
            existingResourceReferences: []);

        // Assert — no outputs were referenced anywhere in main.bicep.
        var kvPath = $"modules/{KeyVaultFolder}/{KeyVaultFile}";
        result.UsedOutputsByModulePath.Should().NotContainKey(kvPath);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static GeneratedTypeModule NewModule(
        string moduleName,
        string logicalName,
        string resourceTypeName,
        string folder,
        string file,
        string resourceGroup,
        IReadOnlyDictionary<string, (string Name, string ResourceTypeName)>? parentIdReferences = null)
    {
        return new GeneratedTypeModule
        {
            ModuleName = moduleName,
            ModuleFileName = file,
            ModuleFolderName = folder,
            ResourceGroupName = resourceGroup,
            LogicalResourceName = logicalName,
            ResourceTypeName = resourceTypeName,
            ResourceAbbreviation = ShortAbbrev(resourceTypeName),
            ParentModuleIdReferences = parentIdReferences
                ?? new Dictionary<string, (string Name, string ResourceTypeName)>(),
        };
    }

    private static ResourceGroupDefinition NewResourceGroup(string name)
    {
        return new ResourceGroupDefinition
        {
            Name = name,
            ResourceAbbreviation = "rg",
        };
    }

    private static string ShortAbbrev(string resourceTypeName) => resourceTypeName switch
    {
        AzureResourceTypes.KeyVault => "kv",
        AzureResourceTypes.WebApp => "app",
        AzureResourceTypes.AppServicePlan => "asp",
        AzureResourceTypes.ContainerApp => "ca",
        AzureResourceTypes.StorageAccount => "stg",
        "UserAssignedIdentity" => "uai",
        _ => "res",
    };
}
