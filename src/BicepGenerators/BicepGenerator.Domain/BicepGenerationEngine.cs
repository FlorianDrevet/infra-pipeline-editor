namespace BicepGenerator.Domain;

public sealed class BicepGenerationEngine
{
    private readonly IEnumerable<IResourceTypeBicepGenerator> _generators;

    public BicepGenerationEngine(
        IEnumerable<IResourceTypeBicepGenerator> generators)
    {
        _generators = generators;
    }

    public GenerationResult Generate(GenerationRequest request)
    {
        var modules = new List<GeneratedTypeModule>();

        foreach (var resource in request.Resources)
        {
            var generator = _generators
                .Single(g => g.ResourceType == resource.Type);

            var module = generator.Generate(resource, request.Environment);

            var resourceIdentifier = BicepIdentifierHelper.ToBicepIdentifier(resource.Name);
            modules.Add(module with
            {
                ModuleName = $"{module.ModuleName}{Capitalize(resourceIdentifier)}",
                ResourceGroupName = resource.ResourceGroupName
            });
        }

        return BicepAssembler.Assemble(modules, request.ResourceGroups);
    }

    private static string Capitalize(string s) =>
        s.Length == 0 ? s : char.ToUpperInvariant(s[0]) + s[1..];
}
