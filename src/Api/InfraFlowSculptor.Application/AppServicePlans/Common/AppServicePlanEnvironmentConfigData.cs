namespace InfraFlowSculptor.Application.AppServicePlans.Common;

/// <summary>
/// Carries typed per-environment App Service Plan configuration data within CQRS commands and results.
/// Only non-null values represent overrides for the target environment.
/// </summary>
/// <param name="EnvironmentName">Name of the environment (e.g., "dev", "staging", "prod").</param>
/// <param name="Sku">Optional pricing tier override (e.g., "S1", "P1v3").</param>
/// <param name="Capacity">Optional number of instances for this environment.</param>
public record AppServicePlanEnvironmentConfigData(
    string EnvironmentName,
    string? Sku,
    int? Capacity);
