namespace InfraFlowSculptor.Contracts.Projects.Responses;

/// <summary>Represents a validated recently viewed item with fresh data from the backend.</summary>
public record RecentItemResponse(
    string Id,
    string Name,
    string Type,
    string? Description);
