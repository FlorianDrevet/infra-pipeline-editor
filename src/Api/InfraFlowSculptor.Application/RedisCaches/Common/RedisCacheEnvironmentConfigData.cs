namespace InfraFlowSculptor.Application.RedisCaches.Common;

/// <summary>
/// Carries typed per-environment Redis Cache configuration data within CQRS commands and results.
/// Only non-null values represent overrides for the target environment.
/// </summary>
public record RedisCacheEnvironmentConfigData(
    string EnvironmentName,
    string? Sku,
    int? Capacity,
    int? RedisVersion,
    bool? EnableNonSslPort,
    string? MinimumTlsVersion,
    string? MaxMemoryPolicy);
