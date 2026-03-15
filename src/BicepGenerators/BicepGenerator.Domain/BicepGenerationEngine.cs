using System.Text;

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
            .GroupBy(r => (r.ResourceGroupName, r.Type));

        var modules = new List<GeneratedTypeModule>();

        foreach (var group in grouped)
        {
            var (rgName, type) = group.Key;

            var generator = _generators
                .Single(g => g.ResourceType == type);

            var module = generator.Generate(
                group.ToList(),
                request.Environment);

            var rgIdentifier = BicepIdentifierHelper.ToBicepIdentifier(rgName);
            modules.Add(module with
            {
                ModuleName = $"{module.ModuleName}{Capitalize(rgIdentifier)}",
                ResourceGroupName = rgName
            });
        }

        return BicepAssembler.Assemble(modules, request.ResourceGroups);
    }

    private static string Capitalize(string s) =>
        s.Length == 0 ? s : char.ToUpperInvariant(s[0]) + s[1..];
}
