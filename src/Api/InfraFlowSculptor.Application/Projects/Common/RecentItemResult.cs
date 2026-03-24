namespace InfraFlowSculptor.Application.Projects.Common;

/// <summary>Application-layer result representing a validated recently viewed item.</summary>
public record RecentItemResult(
    string Id,
    string Name,
    string Type,
    string? Description);
