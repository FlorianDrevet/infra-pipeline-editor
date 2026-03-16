namespace InfraFlowSculptor.Contracts.InfrastructureConfig.Responses;

public record InfrastructureConfigResponse(
    string Id,
    string Name,
    string? DefaultNamingTemplate,
    IReadOnlyList<MemberResponse> Members,
    IReadOnlyList<EnvironmentDefinitionResponse> EnvironmentDefinitions,
    IReadOnlyList<ResourceNamingTemplateResponse> ResourceNamingTemplates);