using InfraFlowSculptor.Application.Common.Interfaces;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Queries.GetBootstrapFileContent;

/// <summary>Query to get the content of a specific generated bootstrap file.</summary>
public record GetBootstrapFileContentQuery(
    Guid InfrastructureConfigId,
    string FilePath
) : IQuery<GetBootstrapFileContentResult>;

/// <summary>Result containing the file content.</summary>
public record GetBootstrapFileContentResult(string Content);
