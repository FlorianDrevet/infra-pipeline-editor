namespace InfraFlowSculptor.BicepGeneration.Ir.Transformations;

/// <summary>
/// Pure transformations on <see cref="BicepModuleSpec"/> for public network access disablement.
/// Injects <c>publicNetworkAccess: 'Disabled'</c> into the resource properties of privatized modules.
/// </summary>
internal static class PublicNetworkAccessTransformer
{
    /// <summary>
    /// Injects <c>publicNetworkAccess: 'Disabled'</c> into the resource body properties.
    /// For resources that support network ACLs (KeyVault, Storage), also injects
    /// <c>networkAcls: { defaultAction: 'Deny' }</c>.
    /// No-op when the property is already present.
    /// </summary>
    internal static BicepModuleSpec WithPublicNetworkAccessDisabled(
        this BicepModuleSpec spec,
        string resourceTypeName)
    {
        var body = spec.Resource.Body.ToList();

        // Find the 'properties' block and inject publicNetworkAccess inside it
        var propsIdx = body.FindIndex(p => p.Key == "properties");
        if (propsIdx >= 0 && body[propsIdx].Value is BicepObjectExpression propsObj)
        {
            // No-op guard: already injected inside properties
            if (propsObj.Properties.Any(p => p.Key == "publicNetworkAccess"))
                return spec;

            var propsList = propsObj.Properties.ToList();

            // Inject publicNetworkAccess: 'Disabled'
            propsList.Add(new BicepPropertyAssignment(
                "publicNetworkAccess",
                new BicepStringLiteral("Disabled")));

            // Inject networkAcls for resources that support it
            if (SupportsNetworkAcls(resourceTypeName))
            {
                propsList.Add(new BicepPropertyAssignment(
                    "networkAcls",
                    new BicepObjectExpression([
                        new BicepPropertyAssignment("defaultAction", new BicepStringLiteral("Deny")),
                    ])));
            }

            body[propsIdx] = new BicepPropertyAssignment(
                "properties",
                new BicepObjectExpression(propsList));
        }
        else
        {
            // No-op guard: already injected at top level
            if (body.Any(p => p.Key == "publicNetworkAccess"))
                return spec;

            // Flat resource body — inject directly at end
            body.Add(new BicepPropertyAssignment(
                "publicNetworkAccess",
                new BicepStringLiteral("Disabled")));
        }

        return spec with
        {
            Resource = spec.Resource with { Body = body },
        };
    }

    private static bool SupportsNetworkAcls(string resourceTypeName) =>
        resourceTypeName is "KeyVault" or "StorageAccount";
}
