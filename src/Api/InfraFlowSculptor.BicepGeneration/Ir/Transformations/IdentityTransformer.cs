namespace InfraFlowSculptor.BicepGeneration.Ir.Transformations;

/// <summary>
/// Pure transformations on <see cref="BicepModuleSpec"/> for identity injection.
/// Replaces the legacy regex-based text manipulation operations.
/// </summary>
internal static class IdentityTransformer
{
    private const string IdentityPropertyName = "identity";
    private const string IdentityTypeParameterName = "identityType";
    private const string ManagedIdentityTypeName = "ManagedIdentityType";
    private const string PrincipalIdOutputName = "principalId";
    private const string PropertiesPropertyName = "properties";
    private const string TypesImportPath = "./types.bicep";
    private const string UserAssignedIdentityIdParameterName = "userAssignedIdentityId";

    /// <summary>
    /// Adds a hard-coded <c>identity: { type: 'SystemAssigned' }</c> block and a
    /// <c>principalId</c> output to the spec. No-op when an identity property already exists.
    /// </summary>
    internal static BicepModuleSpec WithSystemAssignedIdentity(this BicepModuleSpec spec)
    {
        if (spec.Resource.Body.Any(p => p.Key == IdentityPropertyName))
            return spec;

        if (spec.Outputs.Any(o => o.Name == PrincipalIdOutputName))
            return spec;

        var identityBlock = new BicepPropertyAssignment(IdentityPropertyName, new BicepObjectExpression(
        [
            new BicepPropertyAssignment("type", new BicepStringLiteral("SystemAssigned")),
        ]));

        var principalIdOutput = new BicepOutput(
            PrincipalIdOutputName,
            BicepType.String,
            new BicepReference($"{spec.Resource.Symbol}.identity.principalId"));

        return spec with
        {
            Resource = spec.Resource with
            {
                Body = InsertBodyPropertyBefore(spec.Resource.Body, PropertiesPropertyName, identityBlock),
            },
            Outputs = [.. spec.Outputs, principalIdOutput],
        };
    }

    /// <summary>
    /// Adds a <c>UserAssigned</c> or <c>SystemAssigned, UserAssigned</c> identity block
    /// with a <c>userAssignedIdentityId</c> parameter. Upgrades an existing system-assigned
    /// block when <paramref name="alsoHasSystemAssigned"/> is <c>true</c>.
    /// </summary>
    internal static BicepModuleSpec WithUserAssignedIdentity(
        this BicepModuleSpec spec,
        bool alsoHasSystemAssigned)
    {
        var identityType = alsoHasSystemAssigned
            ? "SystemAssigned, UserAssigned"
            : "UserAssigned";

        var newParams = spec.Parameters.ToList();
        if (!spec.Parameters.Any(p => p.Name == UserAssignedIdentityIdParameterName))
        {
            newParams.Add(new BicepParam(
                UserAssignedIdentityIdParameterName,
                BicepType.String,
                "Resource ID of user-assigned managed identity"));
        }

        var identityBody = new BicepObjectExpression(
        [
            new BicepPropertyAssignment("type", new BicepStringLiteral(identityType)),
            new BicepPropertyAssignment("userAssignedIdentities", new BicepObjectExpression(
            [
                new BicepPropertyAssignment("'${userAssignedIdentityId}'", BicepObjectExpression.Empty),
            ])),
        ]);

        var body = spec.Resource.Body.ToList();
        var existingIdx = body.FindIndex(p => p.Key == IdentityPropertyName);
        if (existingIdx >= 0)
        {
            body[existingIdx] = new BicepPropertyAssignment(IdentityPropertyName, identityBody);
        }
        else
        {
            var insertIdx = body.FindIndex(p => p.Key == PropertiesPropertyName);
            if (insertIdx >= 0)
                body.Insert(insertIdx, new BicepPropertyAssignment(IdentityPropertyName, identityBody));
            else
                body.Add(new BicepPropertyAssignment(IdentityPropertyName, identityBody));
        }

        return spec with
        {
            Parameters = newParams,
            Resource = spec.Resource with { Body = body },
        };
    }

    /// <summary>
    /// Adds a parameterized identity block (<c>param identityType ManagedIdentityType</c>) for ARM types
    /// hosting instances with different identity kinds. Adds <c>ManagedIdentityType</c> type import
    /// and type definition.
    /// </summary>
    internal static BicepModuleSpec WithParameterizedIdentity(this BicepModuleSpec spec, bool hasAnyUai)
    {
        if (spec.Resource.Body.Any(p => p.Key == IdentityPropertyName))
            return spec;

        if (spec.Parameters.Any(p => p.Name == IdentityTypeParameterName))
            return spec;

        var newParams = spec.Parameters.ToList();
        newParams.Add(new BicepParam(
            IdentityTypeParameterName,
            BicepType.Custom(ManagedIdentityTypeName),
            "Managed identity type for this resource",
            DefaultValue: new BicepStringLiteral("SystemAssigned")));

        if (hasAnyUai && !spec.Parameters.Any(p => p.Name == UserAssignedIdentityIdParameterName))
        {
            newParams.Add(new BicepParam(
                UserAssignedIdentityIdParameterName,
                BicepType.String,
                "Resource ID of user-assigned managed identity (empty when not applicable)",
                DefaultValue: new BicepStringLiteral("")));
        }

        BicepExpression identityBody;
        if (hasAnyUai)
        {
            identityBody = new BicepObjectExpression(
            [
                new BicepPropertyAssignment("type", new BicepReference("identityType")),
                new BicepPropertyAssignment("userAssignedIdentities",
                    new BicepConditionalExpression(
                        new BicepRawExpression("contains(identityType, 'UserAssigned') && !empty(userAssignedIdentityId)"),
                        new BicepObjectExpression(
                        [
                            new BicepPropertyAssignment("'${userAssignedIdentityId}'", BicepObjectExpression.Empty),
                        ]),
                        new BicepRawExpression("null"))),
            ]);
        }
        else
        {
            identityBody = new BicepObjectExpression(
            [
                new BicepPropertyAssignment("type", new BicepReference("identityType")),
            ]);
        }

        var newImports = spec.Imports.ToList();
        var typesImport = newImports.FindIndex(i => i.Path == TypesImportPath);
        if (typesImport >= 0)
        {
            var existing = newImports[typesImport];
            var symbols = existing.Symbols?.ToList() ?? [];
            if (!symbols.Contains(ManagedIdentityTypeName))
            {
                symbols.Add(ManagedIdentityTypeName);
                newImports[typesImport] = existing with { Symbols = symbols };
            }
        }
        else
        {
            newImports.Insert(0, new BicepImport(TypesImportPath, [ManagedIdentityTypeName]));
        }

        var newOutputs = spec.Outputs.ToList();
        if (!spec.Outputs.Any(o => o.Name == PrincipalIdOutputName))
        {
            newOutputs.Add(new BicepOutput(
                PrincipalIdOutputName,
                BicepType.String,
                new BicepRawExpression($"contains(identityType, 'SystemAssigned') ? {spec.Resource.Symbol}.identity.principalId : ''")));
        }

        var newTypes = spec.ExportedTypes.ToList();
        if (!newTypes.Any(t => t.Name == ManagedIdentityTypeName))
        {
            newTypes.Add(new BicepTypeDefinition(
                ManagedIdentityTypeName,
                new BicepRawExpression("'SystemAssigned' | 'UserAssigned' | 'SystemAssigned, UserAssigned'"),
                IsExported: true,
                "Managed identity type for Azure resources"));
        }

        return spec with
        {
            Imports = newImports,
            Parameters = newParams,
            Resource = spec.Resource with
            {
                Body = InsertBodyPropertyBefore(spec.Resource.Body, PropertiesPropertyName,
                    new BicepPropertyAssignment(IdentityPropertyName, identityBody)),
            },
            Outputs = newOutputs,
            ExportedTypes = newTypes,
        };
    }

    private static IReadOnlyList<BicepPropertyAssignment> InsertBodyPropertyBefore(
        IReadOnlyList<BicepPropertyAssignment> body, string beforeKey, BicepPropertyAssignment newProp)
    {
        var list = body.ToList();
        var index = list.FindIndex(p => p.Key == beforeKey);
        if (index >= 0)
            list.Insert(index, newProp);
        else
            list.Add(newProp);
        return list;
    }
}
