using InfraFlowSculptor.Contracts.Common.Requests;
using InfraFlowSculptor.Contracts.Common.Requests.Profiles;
using InfraFlowSculptor.Domain.Common.OwnedEntities;
using InfraFlowSculptor.Domain.Common.OwnedEntities.Stacks;

namespace InfraFlowSculptor.Application.Common.Helpers;

/// <summary>
/// Maps between <see cref="PipelineStepOptionsDto"/> (contracts) and domain types.
/// </summary>
public static class PipelineStepOptionsDataMapper
{
    public static AppPipelineStepOptionsData ToDomainData(PipelineStepOptionsDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return new AppPipelineStepOptionsData
        {
            Stack = MapStack(dto.Stack),
            Profile = MapProfile(dto.Profile),
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

    public static PipelineStepOptionsDto ToDto(AppPipelineStepOptions entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new PipelineStepOptionsDto
        {
            Stack = entity.Stack == ApplicationStack.Unknown ? null : entity.Stack.Value.ToString(),
            Profile = MapProfileToDto(entity.Profile),
            RunUnitTests = entity.RunUnitTests,
            TestCommand = entity.TestCommand,
            TestFramework = entity.TestFramework,
            TestResultsFormat = entity.TestResultsFormat,
            TestResultsPath = entity.TestResultsPath,
            PublishTestResults = entity.PublishTestResults,
            PublishCodeCoverage = entity.PublishCodeCoverage,
            CoverageTool = entity.CoverageTool,
            CoverageReportPath = entity.CoverageReportPath,
            RunSonarAnalysis = entity.RunSonarAnalysis,
            SonarProjectKey = entity.SonarProjectKey,
            SonarOrganization = entity.SonarOrganization,
            SonarServiceConnection = entity.SonarServiceConnection,
            RunLinting = entity.RunLinting,
            LintCommand = entity.LintCommand,
            RunDependencyScan = entity.RunDependencyScan,
            DependencyScanTool = entity.DependencyScanTool,
            RunBuildValidation = entity.RunBuildValidation,
            EnableDependencyCache = entity.EnableDependencyCache,
            RunSmokeTests = entity.RunSmokeTests,
            SmokeTestCommand = entity.SmokeTestCommand,
        };
    }

    private static ApplicationStack MapStack(string? stack)
    {
        if (string.IsNullOrWhiteSpace(stack))
            return ApplicationStack.Unknown;

        return Enum.TryParse<ApplicationStack.ApplicationStackEnum>(stack, ignoreCase: true, out var parsed)
            ? new ApplicationStack(parsed)
            : ApplicationStack.Unknown;
    }

    private static AppPipelineStackProfile? MapProfile(PipelineStackProfileDto? dto)
    {
        return dto switch
        {
            DotNetProfileDto dotnet => DotNetPipelineProfile.Create(
                new DotNetTestFramework(ParseEnum(dotnet.TestFramework, DotNetTestFramework.DotNetTestFrameworkType.XUnit)),
                dotnet.CollectCoverage,
                dotnet.CustomTestProjectGlob),
            NodeJsProfileDto node => NodeJsPipelineProfile.Create(
                new NodePackageManager(ParseEnum(node.PackageManager, NodePackageManager.NodePackageManagerType.Npm)),
                new NodeTestFramework(ParseEnum(node.TestFramework, NodeTestFramework.NodeTestFrameworkType.Jest)),
                node.RunLintScript,
                node.TestScriptName ?? "test",
                node.LintScriptName ?? "lint"),
            AngularProfileDto angular => AngularPipelineProfile.Create(
                new NodePackageManager(ParseEnum(angular.PackageManager, NodePackageManager.NodePackageManagerType.Npm)),
                angular.RunNgTest,
                angular.RunNgLint,
                angular.RunNgBuildProduction,
                angular.ProjectName),
            JavaProfileDto java => JavaPipelineProfile.Create(
                new JavaBuildTool(ParseEnum(java.BuildTool, JavaBuildTool.JavaBuildToolType.Maven)),
                new JavaTestFramework(ParseEnum(java.TestFramework, JavaTestFramework.JavaTestFrameworkType.JUnit5)),
                java.CollectCoverage),
            PythonProfileDto python => PythonPipelineProfile.Create(
                new PythonPackageManager(ParseEnum(python.PackageManager, PythonPackageManager.PythonPackageManagerType.Pip)),
                new PythonTestFramework(ParseEnum(python.TestFramework, PythonTestFramework.PythonTestFrameworkType.Pytest)),
                python.CollectCoverage),
            StaticSiteProfileDto staticSite => StaticSitePipelineProfile.Create(
                staticSite.BuildCommand ?? string.Empty,
                staticSite.OutputDirectory ?? string.Empty),
            CustomProfileDto custom => CustomPipelineProfile.Create(
                custom.CustomTestCommand,
                custom.CustomLintCommand,
                custom.CustomBuildCommand),
            null => null,
            _ => null,
        };
    }

    private static PipelineStackProfileDto? MapProfileToDto(AppPipelineStackProfile? profile)
    {
        return profile switch
        {
            DotNetPipelineProfile dotnet => new DotNetProfileDto
            {
                TestFramework = dotnet.TestFramework.Value.ToString(),
                CollectCoverage = dotnet.CollectCoverage,
                CustomTestProjectGlob = dotnet.CustomTestProjectGlob,
            },
            NodeJsPipelineProfile node => new NodeJsProfileDto
            {
                PackageManager = node.PackageManager.Value.ToString(),
                TestFramework = node.TestFramework.Value.ToString(),
                RunLintScript = node.RunLintScript,
                TestScriptName = node.TestScriptName,
                LintScriptName = node.LintScriptName,
            },
            AngularPipelineProfile angular => new AngularProfileDto
            {
                PackageManager = angular.PackageManager.Value.ToString(),
                RunNgTest = angular.RunNgTest,
                RunNgLint = angular.RunNgLint,
                RunNgBuildProduction = angular.RunNgBuildProduction,
                ProjectName = angular.ProjectName,
            },
            JavaPipelineProfile java => new JavaProfileDto
            {
                BuildTool = java.BuildTool.Value.ToString(),
                TestFramework = java.TestFramework.Value.ToString(),
                CollectCoverage = java.CollectCoverage,
            },
            PythonPipelineProfile python => new PythonProfileDto
            {
                PackageManager = python.PackageManager.Value.ToString(),
                TestFramework = python.TestFramework.Value.ToString(),
                CollectCoverage = python.CollectCoverage,
            },
            StaticSitePipelineProfile staticSite => new StaticSiteProfileDto
            {
                BuildCommand = staticSite.BuildCommand,
                OutputDirectory = staticSite.OutputDirectory,
            },
            CustomPipelineProfile custom => new CustomProfileDto
            {
                CustomTestCommand = custom.CustomTestCommand,
                CustomLintCommand = custom.CustomLintCommand,
                CustomBuildCommand = custom.CustomBuildCommand,
            },
            null => null,
            _ => null,
        };
    }

    private static TEnum ParseEnum<TEnum>(string? value, TEnum defaultValue)
        where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
            return defaultValue;

        return Enum.TryParse<TEnum>(value, ignoreCase: true, out var result)
            ? result
            : defaultValue;
    }
}
