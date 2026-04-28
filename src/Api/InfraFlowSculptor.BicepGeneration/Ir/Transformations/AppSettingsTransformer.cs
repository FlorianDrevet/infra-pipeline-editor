using InfraFlowSculptor.GenerationCore;

namespace InfraFlowSculptor.BicepGeneration.Ir.Transformations;

/// <summary>
/// Pure transformations on <see cref="BicepModuleSpec"/> for app-settings / env-vars injection.
/// Replaces the legacy regex-based text manipulation operations.
/// </summary>
internal static class AppSettingsTransformer
{
    private const string AppSettingsParameterName = "appSettings";
    private const string ContainersPropertyName = "containers";
    private const string EnvPropertyName = "env";
    private const string EnvVarsParameterName = "envVars";
    private const string PropertiesPropertyName = "properties";
    private const string SiteConfigPropertyName = "siteConfig";
    private const string TemplatePropertyName = "template";

    /// <summary>
    /// Adds <c>param appSettings array = []</c> (Web/Function) or <c>param envVars array = []</c>
    /// (Container App) and wires it into the appropriate property in the resource body.
    /// </summary>
    internal static BicepModuleSpec WithAppSettings(this BicepModuleSpec spec, string armResourceType)
    {
        if (armResourceType == AzureResourceTypes.ArmTypes.ContainerApp)
            return WithContainerAppEnvVars(spec);

        return WithWebFunctionAppSettings(spec);
    }

    private static BicepModuleSpec WithWebFunctionAppSettings(BicepModuleSpec spec)
    {
        if (spec.Parameters.Any(p => p.Name == AppSettingsParameterName))
            return spec;

        var newParams = spec.Parameters.ToList();
        newParams.Add(new BicepParam(
            AppSettingsParameterName,
            BicepType.Array,
            "Application settings (environment variables)",
            DefaultValue: new BicepArrayExpression([])));

        var body = spec.Resource.Body.ToList();
        var propsIdx = body.FindIndex(p => p.Key == PropertiesPropertyName);

        if (propsIdx >= 0 && body[propsIdx].Value is BicepObjectExpression propsObj)
        {
            var propsList = propsObj.Properties.ToList();
            var siteConfigIdx = propsList.FindIndex(p => p.Key == SiteConfigPropertyName);

            if (siteConfigIdx >= 0 && propsList[siteConfigIdx].Value is BicepObjectExpression siteConfigObj)
            {
                var siteConfigProps = siteConfigObj.Properties.ToList();
                var existingAppSettings = siteConfigProps.FindIndex(p => p.Key == AppSettingsParameterName);

                if (existingAppSettings >= 0)
                {
                    // Wrap existing appSettings with concat
                    var existing = siteConfigProps[existingAppSettings].Value;
                    siteConfigProps[existingAppSettings] = new BicepPropertyAssignment(
                        AppSettingsParameterName,
                        new BicepFunctionCall("concat", [existing, new BicepReference(AppSettingsParameterName)]));
                }
                else
                {
                    siteConfigProps.Insert(0, new BicepPropertyAssignment(AppSettingsParameterName, new BicepReference(AppSettingsParameterName)));
                }

                propsList[siteConfigIdx] = new BicepPropertyAssignment(SiteConfigPropertyName, new BicepObjectExpression(siteConfigProps));
            }
            else
            {
                // No siteConfig block yet — add one
                propsList.Add(new BicepPropertyAssignment(SiteConfigPropertyName, new BicepObjectExpression(
                [
                    new BicepPropertyAssignment(AppSettingsParameterName, new BicepReference(AppSettingsParameterName)),
                ])));
            }

            body[propsIdx] = new BicepPropertyAssignment(PropertiesPropertyName, new BicepObjectExpression(propsList));
        }

        return spec with
        {
            Parameters = newParams,
            Resource = spec.Resource with { Body = body },
        };
    }

    private static BicepModuleSpec WithContainerAppEnvVars(BicepModuleSpec spec)
    {
        if (spec.Parameters.Any(p => p.Name == EnvVarsParameterName))
            return spec;

        var newParams = spec.Parameters.ToList();
        newParams.Add(new BicepParam(
            EnvVarsParameterName,
            BicepType.Array,
            "Environment variables for the container",
            DefaultValue: new BicepArrayExpression([])));

        // Wire envVars into properties.template.containers[0].env
        var body = spec.Resource.Body.ToList();
        var propsIdx = body.FindIndex(p => p.Key == PropertiesPropertyName);

        if (propsIdx >= 0 && body[propsIdx].Value is BicepObjectExpression propsObj)
        {
            var propsList = propsObj.Properties.ToList();
            var templateIdx = propsList.FindIndex(p => p.Key == TemplatePropertyName);

            if (templateIdx >= 0 && propsList[templateIdx].Value is BicepObjectExpression templateObj)
            {
                var templateProps = templateObj.Properties.ToList();
                var containersIdx = templateProps.FindIndex(p => p.Key == ContainersPropertyName);

                if (containersIdx >= 0
                    && templateProps[containersIdx].Value is BicepArrayExpression containersArr
                    && containersArr.Items.Count > 0
                    && containersArr.Items[0] is BicepObjectExpression firstContainer)
                {
                    var containerProps = firstContainer.Properties.ToList();
                    containerProps.Add(new BicepPropertyAssignment(EnvPropertyName, new BicepReference(EnvVarsParameterName)));

                    var newElements = containersArr.Items.ToList();
                    newElements[0] = new BicepObjectExpression(containerProps);

                    templateProps[containersIdx] = new BicepPropertyAssignment(
                        ContainersPropertyName, new BicepArrayExpression(newElements));
                }

                propsList[templateIdx] = new BicepPropertyAssignment(TemplatePropertyName, new BicepObjectExpression(templateProps));
            }

            body[propsIdx] = new BicepPropertyAssignment(PropertiesPropertyName, new BicepObjectExpression(propsList));
        }

        return spec with
        {
            Parameters = newParams,
            Resource = spec.Resource with { Body = body },
        };
    }
}
