using InfraFlowSculptor.Contracts.Common.Requests;
using InfraFlowSculptor.Domain.Common.OwnedEntities;

namespace InfraFlowSculptor.Application.Common.Helpers;

internal static class PipelineStepOptionsDataMapper
{
    public static AppPipelineStepOptionsData ToDomainData(PipelineStepOptionsDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return new AppPipelineStepOptionsData
        {
            RunUnitTests = dto.RunUnitTests,
            TestCommand = dto.TestCommand,
            TestFramework = dto.TestFramework,
            TestResultsFormat = dto.TestResultsFormat,
            TestResultsPath = dto.TestResultsPath,
            PublishTestResults = dto.PublishTestResults,
            PublishCodeCoverage = dto.PublishCodeCoverage,
            CoverageTool = dto.CoverageTool,
            CoverageReportPath = dto.CoverageReportPath,
            RunSonarAnalysis = dto.RunSonarAnalysis,
            SonarProjectKey = dto.SonarProjectKey,
            SonarOrganization = dto.SonarOrganization,
            SonarServiceConnection = dto.SonarServiceConnection,
            RunLinting = dto.RunLinting,
            LintCommand = dto.LintCommand,
            RunDependencyScan = dto.RunDependencyScan,
            DependencyScanTool = dto.DependencyScanTool,
            RunBuildValidation = dto.RunBuildValidation,
            EnableDependencyCache = dto.EnableDependencyCache,
            RunSmokeTests = dto.RunSmokeTests,
            SmokeTestCommand = dto.SmokeTestCommand,
        };
    }
}