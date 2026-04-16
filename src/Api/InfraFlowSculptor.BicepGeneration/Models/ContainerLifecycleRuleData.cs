namespace InfraFlowSculptor.BicepGeneration.Models;

/// <summary>
/// Carries a single blob lifecycle management rule for the blob service companion Bicep module.
/// </summary>
public sealed record ContainerLifecycleRuleData(
    string RuleName,
    IReadOnlyCollection<string> ContainerNames,
    int TimeToLiveInDays);
