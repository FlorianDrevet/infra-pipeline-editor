using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Builder;

namespace InfraFlowSculptor.BicepGeneration.Generators.Helpers;

/// <summary>
/// Builds ACR-related Bicep IR nodes (secrets, registries) for the Container App generator.
/// </summary>
internal static class ContainerAppAcrBicepHelper
{
    internal const string AcrLoginServerParameterName = "acrLoginServer";
    internal const string AcrPasswordParameterName = "acrPassword";
    internal const string AcrManagedIdentityClientIdParameterName = "acrManagedIdentityClientId";
    internal const string AcrUsernameVariableName = "acrUsername";
    internal const string AcrPasswordSecretNameVariableName = "acrPasswordSecretName";
    internal const string AcrPasswordSecretNameValue = "acr-password";
    internal const string AcrUsernameExpression = "split(acrLoginServer, '.')[0]";
    internal const string ManagedIdentityClientIdConditionExpression = "!empty(acrManagedIdentityClientId)";

    private const string SystemManagedIdentityValue = "system";
    private const string SecretsPropertyName = "secrets";
    private const string RegistriesPropertyName = "registries";
    private const string ServerPropertyName = "server";
    private const string UsernamePropertyName = "username";
    private const string PasswordSecretRefPropertyName = "passwordSecretRef";
    private const string IdentityPropertyName = "identity";
    private const string NamePropertyName = "name";
    private const string ValuePropertyName = "value";
    private const string EmptyParameterValue = "";

    /// <summary>
    /// Adds ACR-related parameters to the Bicep module builder.
    /// </summary>
    internal static BicepModuleBuilder AddAcrParameters(
        this BicepModuleBuilder builder,
        bool useAdminCredentials)
    {
        builder.Param(AcrLoginServerParameterName, BicepType.String,
            "ACR login server (e.g. myregistry.azurecr.io)");

        if (useAdminCredentials)
        {
            builder.Param(AcrPasswordParameterName, BicepType.String,
                "Admin password for the Container Registry", secure: true);
        }
        else
        {
            builder.Param(AcrManagedIdentityClientIdParameterName, BicepType.String,
                "Client ID of the managed identity for ACR pull",
                defaultValue: new BicepStringLiteral(EmptyParameterValue));
        }

        return builder;
    }

    /// <summary>
    /// Adds ACR-related variables (username, secret name) for admin credentials mode.
    /// </summary>
    internal static BicepModuleBuilder AddAcrAdminVariables(this BicepModuleBuilder builder)
    {
        builder.Var(AcrUsernameVariableName, new BicepRawExpression(AcrUsernameExpression));
        builder.Var(AcrPasswordSecretNameVariableName, new BicepStringLiteral(AcrPasswordSecretNameValue));
        return builder;
    }

    /// <summary>
    /// Builds the ACR configuration properties (secrets + registries) for the resource's configuration block.
    /// </summary>
    internal static List<BicepPropertyAssignment> BuildAcrConfigProperties(bool useAdminCredentials)
    {
        var props = new List<BicepPropertyAssignment>();

        if (useAdminCredentials)
        {
            props.Add(new BicepPropertyAssignment(SecretsPropertyName, new BicepArrayExpression([
                new BicepObjectExpression([
                    new BicepPropertyAssignment(NamePropertyName, new BicepReference(AcrPasswordSecretNameVariableName)),
                    new BicepPropertyAssignment(ValuePropertyName, new BicepReference(AcrPasswordParameterName)),
                ]),
            ])));
            props.Add(new BicepPropertyAssignment(RegistriesPropertyName, new BicepArrayExpression([
                new BicepObjectExpression([
                    new BicepPropertyAssignment(ServerPropertyName, new BicepReference(AcrLoginServerParameterName)),
                    new BicepPropertyAssignment(UsernamePropertyName, new BicepReference(AcrUsernameVariableName)),
                    new BicepPropertyAssignment(PasswordSecretRefPropertyName, new BicepReference(AcrPasswordSecretNameVariableName)),
                ]),
            ])));
        }
        else
        {
            props.Add(new BicepPropertyAssignment(RegistriesPropertyName, new BicepArrayExpression([
                new BicepObjectExpression([
                    new BicepPropertyAssignment(ServerPropertyName, new BicepReference(AcrLoginServerParameterName)),
                    new BicepPropertyAssignment(IdentityPropertyName, new BicepConditionalExpression(
                        new BicepRawExpression(ManagedIdentityClientIdConditionExpression),
                        new BicepReference(AcrManagedIdentityClientIdParameterName),
                        new BicepStringLiteral(SystemManagedIdentityValue))),
                ]),
            ])));
        }

        return props;
    }
}
