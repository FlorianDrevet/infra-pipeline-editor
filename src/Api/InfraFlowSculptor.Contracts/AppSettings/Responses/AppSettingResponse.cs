namespace InfraFlowSculptor.Contracts.AppSettings.Responses;

/// <summary>Response DTO for an app setting.</summary>
public record AppSettingResponse(
    string Id,
    string ResourceId,
    string Name,
    string? StaticValue,
    string? SourceResourceId,
    string? SourceOutputName,
    bool IsOutputReference);
