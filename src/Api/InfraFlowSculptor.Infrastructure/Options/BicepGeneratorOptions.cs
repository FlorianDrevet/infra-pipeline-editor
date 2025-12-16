using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Infrastructure.Options;

public sealed class BicepGeneratorOptions
{
    public static string SectionName = "BicepGenerator";

    [Required]
    public required Uri BaseUri { get; set; }
}