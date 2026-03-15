namespace BicepGenerator.Domain;

public class ResourceDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string ResourceGroupName { get; set; } = string.Empty;
    public IReadOnlyDictionary<string, string> Properties { get; set; } =
        new Dictionary<string, string>();
}
