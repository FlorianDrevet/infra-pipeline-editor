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
        var grouped = request.Resources
            .GroupBy(r => r.Type);

        var modules = new List<GeneratedTypeModule>();

        foreach (var group in grouped)
        {
            var generator = _generators
                .Single(g => g.ResourceType == group.Key);

            modules.Add(generator.Generate(
                group.ToList(),
                request.Environment));
        }

        return BicepAssembler.Assemble(modules);
    }
}
