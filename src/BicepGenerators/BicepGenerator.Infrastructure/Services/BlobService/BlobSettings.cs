namespace BicepGenerator.Infrastructure.Services.BlobService;

public class BlobSettings
{
    public const string SectionName = "BlobSettings";
    public string ContainerName { get; init; } = null!;
}