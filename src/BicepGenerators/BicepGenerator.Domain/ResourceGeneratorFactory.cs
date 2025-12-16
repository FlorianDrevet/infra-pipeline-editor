namespace BicepGenerator.Domain;

public sealed class ResourceGeneratorFactory
{
    private readonly Dictionary<string, IResourceTypeBicepGenerator> _generators;

    public ResourceGeneratorFactory(IEnumerable<IResourceTypeBicepGenerator> generators)
    {
        _generators = generators.ToDictionary(g => g.ResourceType);
    }

    public IResourceTypeBicepGenerator Resolve(string resourceType)
    {
        if (!_generators.TryGetValue(resourceType, out var generator))
            throw new NotSupportedException($"Resource type '{resourceType}' not supported");

        return generator;
    }
}
