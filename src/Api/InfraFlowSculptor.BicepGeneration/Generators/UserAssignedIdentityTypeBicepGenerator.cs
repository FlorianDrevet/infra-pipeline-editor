using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Builder;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Models;
using static InfraFlowSculptor.BicepGeneration.Generators.Constants.BicepGeneratorSharedConstants;

namespace InfraFlowSculptor.BicepGeneration.Generators;

/// <summary>
/// Generates a Bicep module for a <c>Microsoft.ManagedIdentity/userAssignedIdentities</c> resource.
/// Migrated to Builder + IR (Vague 2).
/// </summary>
public sealed class UserAssignedIdentityTypeBicepGenerator : IResourceTypeBicepSpecGenerator
{
    private const string ModuleName = "userAssignedIdentity";
    private const string ModuleFolderName = "UserAssignedIdentity";
    private const string ModuleFileName = "userAssignedIdentity";
    private const string ResourceSymbol = "identity";
    private const string UserAssignedIdentityArmType = "Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31";
    private const string ResourceIdOutputName = "resourceId";
    private const string PrincipalIdOutputName = "principalId";
    private const string ClientIdOutputName = "clientId";
    private const string ResourceIdExpression = ResourceSymbol + ".id";
    private const string PrincipalIdExpression = ResourceSymbol + ".properties.principalId";
    private const string ClientIdExpression = ResourceSymbol + ".properties.clientId";

    /// <inheritdoc />
    public string ResourceType => AzureResourceTypes.ArmTypes.UserAssignedIdentityType;

    /// <inheritdoc />
    public string ResourceTypeName => AzureResourceTypes.UserAssignedIdentity;

    /// <inheritdoc />
    public BicepModuleSpec GenerateSpec(ResourceDefinition resource)
    {
        return new BicepModuleBuilder()
            .Module(ModuleName, ModuleFolderName, AzureResourceTypes.UserAssignedIdentity)
            .Param(LocationParameterName, BicepType.String, description: "Azure region for the User Assigned Identity")
            .Param(NameParameterName, BicepType.String, description: "Name of the User Assigned Identity")
            .Resource(ResourceSymbol, UserAssignedIdentityArmType)
            .Property(NamePropertyName, new BicepReference(NameParameterName))
            .Property(LocationPropertyName, new BicepReference(LocationParameterName))
            .Output(ResourceIdOutputName, BicepType.String, new BicepRawExpression(ResourceIdExpression))
            .Output(PrincipalIdOutputName, BicepType.String, new BicepRawExpression(PrincipalIdExpression))
            .Output(ClientIdOutputName, BicepType.String, new BicepRawExpression(ClientIdExpression))
            .Build();
    }

    /// <inheritdoc />
    public GeneratedTypeModule Generate(ResourceDefinition resource)
    {
        return new GeneratedTypeModule
        {
            ModuleName = ModuleName,
            ModuleFileName = ModuleFileName,
            ModuleFolderName = ModuleFolderName,
            ModuleBicepContent = UserAssignedIdentityModuleTemplate,
            ResourceTypeName = ResourceTypeName,
            Parameters = new Dictionary<string, object>()
        };
    }

    private const string UserAssignedIdentityModuleTemplate = """
        @description('Azure region for the User Assigned Identity')
        param location string

        @description('Name of the User Assigned Identity')
        param name string

        resource identity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
          name: name
          location: location
        }

        output resourceId string = identity.id
        output principalId string = identity.properties.principalId
        output clientId string = identity.properties.clientId
        """;
}
