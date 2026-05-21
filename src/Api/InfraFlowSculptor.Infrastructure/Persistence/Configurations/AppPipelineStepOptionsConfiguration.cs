using InfraFlowSculptor.Domain.Common.OwnedEntities;
using InfraFlowSculptor.Domain.Common.OwnedEntities.Stacks;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

/// <summary>
/// Shared EF Core configuration for the <see cref="AppPipelineStepOptions"/> owned entity.
/// Applied identically on WebApp, FunctionApp, and ContainerApp tables.
/// </summary>
internal static class AppPipelineStepOptionsConfiguration
{
    private const int CommandMaxLength = 500;
    private const int ToolMaxLength = 50;
    private const int KeyMaxLength = 200;
    private const int PathMaxLength = 500;
    private const int FormatMaxLength = 20;
    private const int StackMaxLength = 30;
    private const string ProfileStackPropertyName = "ProfileStack";
    private const string DotNetProfileTestFrameworkPropertyName = "DotNetProfileTestFramework";
    private const string DotNetProfileCollectCoveragePropertyName = "DotNetProfileCollectCoverage";
    private const string DotNetProfileCustomTestProjectGlobPropertyName = "DotNetProfileCustomTestProjectGlob";
    private const string NodeJsProfilePackageManagerPropertyName = "NodeJsProfilePackageManager";
    private const string NodeJsProfileTestFrameworkPropertyName = "NodeJsProfileTestFramework";
    private const string NodeJsProfileRunLintScriptPropertyName = "NodeJsProfileRunLintScript";
    private const string NodeJsProfileTestScriptNamePropertyName = "NodeJsProfileTestScriptName";
    private const string NodeJsProfileLintScriptNamePropertyName = "NodeJsProfileLintScriptName";
    private const string AngularProfilePackageManagerPropertyName = "AngularProfilePackageManager";
    private const string AngularProfileRunNgTestPropertyName = "AngularProfileRunNgTest";
    private const string AngularProfileRunNgLintPropertyName = "AngularProfileRunNgLint";
    private const string AngularProfileRunNgBuildProductionPropertyName = "AngularProfileRunNgBuildProduction";
    private const string AngularProfileProjectNamePropertyName = "AngularProfileProjectName";
    private const string JavaProfileBuildToolPropertyName = "JavaProfileBuildTool";
    private const string JavaProfileTestFrameworkPropertyName = "JavaProfileTestFramework";
    private const string JavaProfileCollectCoveragePropertyName = "JavaProfileCollectCoverage";
    private const string PythonProfilePackageManagerPropertyName = "PythonProfilePackageManager";
    private const string PythonProfileTestFrameworkPropertyName = "PythonProfileTestFramework";
    private const string PythonProfileCollectCoveragePropertyName = "PythonProfileCollectCoverage";
    private const string StaticSiteProfileBuildCommandPropertyName = "StaticSiteProfileBuildCommand";
    private const string StaticSiteProfileOutputDirectoryPropertyName = "StaticSiteProfileOutputDirectory";
    private const string CustomProfileTestCommandPropertyName = "CustomProfileTestCommand";
    private const string CustomProfileLintCommandPropertyName = "CustomProfileLintCommand";
    private const string CustomProfileBuildCommandPropertyName = "CustomProfileBuildCommand";

    /// <summary>Configures the owned <see cref="AppPipelineStepOptions"/> entity on a compute aggregate.</summary>
    internal static void Configure<TOwner>(OwnedNavigationBuilder<TOwner, AppPipelineStepOptions> options)
        where TOwner : class
    {
        options.Ignore(o => o.Profile);

        // ── Application stack profile ──────────────────────────
        options.Property(o => o.Stack)
            .HasConversion(new EnumValueConverter<ApplicationStack, ApplicationStack.ApplicationStackEnum>())
            .HasMaxLength(StackMaxLength)
            .HasDefaultValue(ApplicationStack.Unknown)
            .IsRequired();

        options.Property<ApplicationStack?>(ProfileStackPropertyName)
            .HasConversion(new NullableEnumValueConverter<ApplicationStack, ApplicationStack.ApplicationStackEnum>())
            .HasMaxLength(StackMaxLength)
            .IsRequired(false);

        ConfigureDotNetProfile(options);
        ConfigureNodeJsProfile(options);
        ConfigureAngularProfile(options);
        ConfigureJavaProfile(options);
        ConfigurePythonProfile(options);
        ConfigureStaticSiteProfile(options);
        ConfigureCustomProfile(options);

        // ── Tests ──────────────────────────────────────────────
        options.Property(o => o.RunUnitTests).HasDefaultValue(false);
        options.Property(o => o.TestCommand).HasMaxLength(CommandMaxLength).IsRequired(false);
        options.Property(o => o.TestFramework).HasMaxLength(ToolMaxLength).IsRequired(false);
        options.Property(o => o.TestResultsFormat).HasMaxLength(FormatMaxLength).IsRequired(false);
        options.Property(o => o.TestResultsPath).HasMaxLength(PathMaxLength).IsRequired(false);
        options.Property(o => o.PublishTestResults).HasDefaultValue(false);

        // ── Coverage ───────────────────────────────────────────
        options.Property(o => o.PublishCodeCoverage).HasDefaultValue(false);
        options.Property(o => o.CoverageTool).HasMaxLength(FormatMaxLength).IsRequired(false);
        options.Property(o => o.CoverageReportPath).HasMaxLength(PathMaxLength).IsRequired(false);

        // ── Sonar ──────────────────────────────────────────────
        options.Property(o => o.RunSonarAnalysis).HasDefaultValue(false);
        options.Property(o => o.SonarProjectKey).HasMaxLength(KeyMaxLength).IsRequired(false);
        options.Property(o => o.SonarOrganization).HasMaxLength(KeyMaxLength).IsRequired(false);
        options.Property(o => o.SonarServiceConnection).HasMaxLength(KeyMaxLength).IsRequired(false);

        // ── Linting ────────────────────────────────────────────
        options.Property(o => o.RunLinting).HasDefaultValue(false);
        options.Property(o => o.LintCommand).HasMaxLength(CommandMaxLength).IsRequired(false);

        // ── Security ───────────────────────────────────────────
        options.Property(o => o.RunDependencyScan).HasDefaultValue(false);
        options.Property(o => o.DependencyScanTool).HasMaxLength(ToolMaxLength).IsRequired(false);

        // ── Build ──────────────────────────────────────────────
        options.Property(o => o.RunBuildValidation).HasDefaultValue(false);

        // ── Cache ──────────────────────────────────────────────
        options.Property(o => o.EnableDependencyCache).HasDefaultValue(false);

        // ── Post-deploy ────────────────────────────────────────
        options.Property(o => o.RunSmokeTests).HasDefaultValue(false);
        options.Property(o => o.SmokeTestCommand).HasMaxLength(CommandMaxLength).IsRequired(false);
    }

    private static void ConfigureDotNetProfile<TOwner>(OwnedNavigationBuilder<TOwner, AppPipelineStepOptions> options)
        where TOwner : class
    {
        options.Property<DotNetTestFramework?>(DotNetProfileTestFrameworkPropertyName)
            .HasConversion(new NullableEnumValueConverter<DotNetTestFramework, DotNetTestFramework.DotNetTestFrameworkType>())
            .HasMaxLength(ToolMaxLength)
            .IsRequired(false);
        options.Property<bool?>(DotNetProfileCollectCoveragePropertyName).IsRequired(false);
        options.Property<string?>(DotNetProfileCustomTestProjectGlobPropertyName)
            .HasMaxLength(CommandMaxLength)
            .IsRequired(false);
    }

    private static void ConfigureNodeJsProfile<TOwner>(OwnedNavigationBuilder<TOwner, AppPipelineStepOptions> options)
        where TOwner : class
    {
        options.Property<NodePackageManager?>(NodeJsProfilePackageManagerPropertyName)
            .HasConversion(new NullableEnumValueConverter<NodePackageManager, NodePackageManager.NodePackageManagerType>())
            .HasMaxLength(ToolMaxLength)
            .IsRequired(false);
        options.Property<NodeTestFramework?>(NodeJsProfileTestFrameworkPropertyName)
            .HasConversion(new NullableEnumValueConverter<NodeTestFramework, NodeTestFramework.NodeTestFrameworkType>())
            .HasMaxLength(ToolMaxLength)
            .IsRequired(false);
        options.Property<bool?>(NodeJsProfileRunLintScriptPropertyName).IsRequired(false);
        options.Property<string?>(NodeJsProfileTestScriptNamePropertyName).HasMaxLength(ToolMaxLength).IsRequired(false);
        options.Property<string?>(NodeJsProfileLintScriptNamePropertyName).HasMaxLength(ToolMaxLength).IsRequired(false);
    }

    private static void ConfigureAngularProfile<TOwner>(OwnedNavigationBuilder<TOwner, AppPipelineStepOptions> options)
        where TOwner : class
    {
        options.Property<NodePackageManager?>(AngularProfilePackageManagerPropertyName)
            .HasConversion(new NullableEnumValueConverter<NodePackageManager, NodePackageManager.NodePackageManagerType>())
            .HasMaxLength(ToolMaxLength)
            .IsRequired(false);
        options.Property<bool?>(AngularProfileRunNgTestPropertyName).IsRequired(false);
        options.Property<bool?>(AngularProfileRunNgLintPropertyName).IsRequired(false);
        options.Property<bool?>(AngularProfileRunNgBuildProductionPropertyName).IsRequired(false);
        options.Property<string?>(AngularProfileProjectNamePropertyName).HasMaxLength(KeyMaxLength).IsRequired(false);
    }

    private static void ConfigureJavaProfile<TOwner>(OwnedNavigationBuilder<TOwner, AppPipelineStepOptions> options)
        where TOwner : class
    {
        options.Property<JavaBuildTool?>(JavaProfileBuildToolPropertyName)
            .HasConversion(new NullableEnumValueConverter<JavaBuildTool, JavaBuildTool.JavaBuildToolType>())
            .HasMaxLength(ToolMaxLength)
            .IsRequired(false);
        options.Property<JavaTestFramework?>(JavaProfileTestFrameworkPropertyName)
            .HasConversion(new NullableEnumValueConverter<JavaTestFramework, JavaTestFramework.JavaTestFrameworkType>())
            .HasMaxLength(ToolMaxLength)
            .IsRequired(false);
        options.Property<bool?>(JavaProfileCollectCoveragePropertyName).IsRequired(false);
    }

    private static void ConfigurePythonProfile<TOwner>(OwnedNavigationBuilder<TOwner, AppPipelineStepOptions> options)
        where TOwner : class
    {
        options.Property<PythonPackageManager?>(PythonProfilePackageManagerPropertyName)
            .HasConversion(new NullableEnumValueConverter<PythonPackageManager, PythonPackageManager.PythonPackageManagerType>())
            .HasMaxLength(ToolMaxLength)
            .IsRequired(false);
        options.Property<PythonTestFramework?>(PythonProfileTestFrameworkPropertyName)
            .HasConversion(new NullableEnumValueConverter<PythonTestFramework, PythonTestFramework.PythonTestFrameworkType>())
            .HasMaxLength(ToolMaxLength)
            .IsRequired(false);
        options.Property<bool?>(PythonProfileCollectCoveragePropertyName).IsRequired(false);
    }

    private static void ConfigureStaticSiteProfile<TOwner>(OwnedNavigationBuilder<TOwner, AppPipelineStepOptions> options)
        where TOwner : class
    {
        options.Property<string?>(StaticSiteProfileBuildCommandPropertyName).HasMaxLength(CommandMaxLength).IsRequired(false);
        options.Property<string?>(StaticSiteProfileOutputDirectoryPropertyName).HasMaxLength(PathMaxLength).IsRequired(false);
    }

    private static void ConfigureCustomProfile<TOwner>(OwnedNavigationBuilder<TOwner, AppPipelineStepOptions> options)
        where TOwner : class
    {
        options.Property<string?>(CustomProfileTestCommandPropertyName).HasMaxLength(CommandMaxLength).IsRequired(false);
        options.Property<string?>(CustomProfileLintCommandPropertyName).HasMaxLength(CommandMaxLength).IsRequired(false);
        options.Property<string?>(CustomProfileBuildCommandPropertyName).HasMaxLength(CommandMaxLength).IsRequired(false);
    }
}
