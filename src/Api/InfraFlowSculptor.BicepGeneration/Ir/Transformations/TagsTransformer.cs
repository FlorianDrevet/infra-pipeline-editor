namespace InfraFlowSculptor.BicepGeneration.Ir.Transformations;

/// <summary>
/// Pure transformations on <see cref="BicepModuleSpec"/> for tags injection.
/// Replaces the legacy <see cref="TextManipulation.BicepTagsInjector"/> regex operations.
/// </summary>
internal static class TagsTransformer
{
    /// <summary>
    /// Adds <c>param tags object = {}</c> and <c>tags: tags</c> property on the resource body
    /// after <c>location</c>. No-op when tags are already declared.
    /// </summary>
    internal static BicepModuleSpec WithTags(this BicepModuleSpec spec)
    {
        if (spec.Parameters.Any(p => p.Name == "tags"))
            return spec;

        var newParams = spec.Parameters.ToList();
        newParams.Add(new BicepParam(
            "tags",
            BicepType.Object,
            "Resource tags",
            DefaultValue: BicepObjectExpression.Empty));

        var body = spec.Resource.Body.ToList();
        var locationIdx = body.FindIndex(p => p.Key == "location");
        var insertIdx = locationIdx >= 0 ? locationIdx + 1 : body.Count;
        body.Insert(insertIdx, new BicepPropertyAssignment("tags", new BicepReference("tags")));

        return spec with
        {
            Parameters = newParams,
            Resource = spec.Resource with { Body = body },
        };
    }
}
