using InfraFlowSculptor.Domain.Common.OwnedEntities;
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

    /// <summary>Configures the owned <see cref="AppPipelineStepOptions"/> entity on a compute aggregate.</summary>
    internal static void Configure<TOwner>(OwnedNavigationBuilder<TOwner, AppPipelineStepOptions> options)
        where TOwner : class
    {
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
}
