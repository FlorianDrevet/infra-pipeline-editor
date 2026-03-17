namespace InfraFlowSculptor.Domain.Common.AzureRoleDefinitions;

public record AzureRoleDefinition(
    string Id,
    string Name,
    string Description,
    string DocumentationUrl
);
