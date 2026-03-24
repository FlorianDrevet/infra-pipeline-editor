using System.ComponentModel.DataAnnotations;
using InfraFlowSculptor.Contracts.ValidationAttributes;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.FunctionAppAggregate.ValueObjects;

namespace InfraFlowSculptor.Contracts.FunctionApps.Requests;

/// <summary>Common properties shared by create and update Function App requests.</summary>
public abstract class FunctionAppRequestBase
{
    /// <summary>Display name for the Function App resource.</summary>
    [Required]
    public required string Name { get; init; }

    /// <summary>Azure region where the Function App will be deployed.</summary>
    [Required, EnumValidation(typeof(Location.LocationEnum))]
    public required string Location { get; init; }

    /// <summary>Identifier of the App Service Plan that hosts this Function App.</summary>
    [Required, GuidValidation]
    public required Guid AppServicePlanId { get; init; }

    /// <summary>Runtime stack (DotNet, Node, Python, Java, PowerShell).</summary>
    [Required, EnumValidation(typeof(FunctionAppRuntimeStack.FunctionAppRuntimeStackEnum))]
    public required string RuntimeStack { get; init; }

    /// <summary>Runtime version (e.g., "8.0", "20").</summary>
    [Required]
    public required string RuntimeVersion { get; init; }

    /// <summary>Whether the app requires HTTPS only.</summary>
    public bool HttpsOnly { get; init; } = true;

    /// <summary>Per-environment typed configuration overrides.</summary>
    public List<FunctionAppEnvironmentConfigEntry>? EnvironmentSettings { get; init; }
}

/// <summary>Typed per-environment configuration entry for a Function App.</summary>
public class FunctionAppEnvironmentConfigEntry
{
    /// <summary>Name of the target environment (e.g., "dev", "staging", "prod").</summary>
    [Required]
    public required string EnvironmentName { get; init; }

    /// <summary>Optional HTTPS-only override.</summary>
    public bool? HttpsOnly { get; init; }

    /// <summary>Optional runtime stack override.</summary>
    [EnumValidation(typeof(FunctionAppRuntimeStack.FunctionAppRuntimeStackEnum))]
    public string? RuntimeStack { get; init; }

    /// <summary>Optional runtime version override.</summary>
    public string? RuntimeVersion { get; init; }

    /// <summary>Optional maximum scale-out instance count override.</summary>
    public int? MaxInstanceCount { get; init; }

    /// <summary>Optional Functions worker runtime override (e.g., "dotnet-isolated", "node", "python").</summary>
    public string? FunctionsWorkerRuntime { get; init; }
}

/// <summary>Response DTO for a typed per-environment Function App configuration.</summary>
public record FunctionAppEnvironmentConfigResponse(
    string EnvironmentName,
    bool? HttpsOnly,
    string? RuntimeStack,
    string? RuntimeVersion,
    int? MaxInstanceCount,
    string? FunctionsWorkerRuntime);
