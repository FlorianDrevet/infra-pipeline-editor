using InfraFlowSculptor.GenerationCore;

namespace InfraFlowSculptor.BicepGeneration.Ir.Transformations;

/// <summary>
/// Pure transformations on <see cref="BicepModuleSpec"/> for app-settings / env-vars injection.
/// Replaces the legacy regex-based text manipulation operations.
/// </summary>
internal static class AppSettingsTransformer
{
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
        if (spec.Parameters.Any(p => p.Name == "appSettings"))
            return spec;

        var newParams = spec.Parameters.ToList();
        newParams.Add(new BicepParam(
            "appSettings",
            BicepType.Array,
            "Application settings (environment variables)",
            DefaultValue: new BicepArrayExpression([])));

        var body = spec.Resource.Body.ToList();
        var propsIdx = body.FindIndex(p => p.Key == "properties");

        if (propsIdx >= 0 && body[propsIdx].Value is BicepObjectExpression propsObj)
        {
            var propsList = propsObj.Properties.ToList();
            var siteConfigIdx = propsList.FindIndex(p => p.Key == "siteConfig");

            if (siteConfigIdx >= 0 && propsList[siteConfigIdx].Value is BicepObjectExpression siteConfigObj)
            {
                var siteConfigProps = siteConfigObj.Properties.ToList();
                var existingAppSettings = siteConfigProps.FindIndex(p => p.Key == "appSettings");

                if (existingAppSettings >= 0)
                {
                    // Wrap existing appSettings with concat
                    var existing = siteConfigProps[existingAppSettings].Value;
                    siteConfigProps[existingAppSettings] = new BicepPropertyAssignment(
                        "appSettings",
                        new BicepFunctionCall("concat", [existing, new BicepReference("appSettings")]));
                }
                else
                {
                    siteConfigProps.Insert(0, new BicepPropertyAssignment("appSettings", new BicepReference("appSettings")));
                }

                propsList[siteConfigIdx] = new BicepPropertyAssignment("siteConfig", new BicepObjectExpression(siteConfigProps));
            }
            else
            {
                // No siteConfig block yet — add one
                propsList.Add(new BicepPropertyAssignment("siteConfig", new BicepObjectExpression(
                [
                    new BicepPropertyAssignment("appSettings", new BicepReference("appSettings")),
                ])));
            }

            body[propsIdx] = new BicepPropertyAssignment("properties", new BicepObjectExpression(propsList));
        }

        return spec with
        {
            Parameters = newParams,
            Resource = spec.Resource with { Body = body },
        };
    }

    private static BicepModuleSpec WithContainerAppEnvVars(BicepModuleSpec spec)
    {
        if (spec.Parameters.Any(p => p.Name == "envVars"))
            return spec;

        var newParams = spec.Parameters.ToList();
        newParams.Add(new BicepParam(
            "envVars",
            BicepType.Array,
            "Environment variables for the container",
            DefaultValue: new BicepArrayExpression([])));

        // Container App env injection is wired into the container spec via the resource body.
        // The exact wiring depends on the generator's body structure, which will be handled
        // during each generator's migration. For Phase 0, we just add the parameter.
        return spec with
        {
            Parameters = newParams,
        };
    }
}
