namespace InfraFlowSculptor.PipelineGeneration.Tests.Architecture;

public sealed class PipelineSourceFileSizeGuardTests
{
    private const string TestProjectMarker = "InfraFlowSculptor.PipelineGeneration.Tests.csproj";
    private const int PhaseP2LineThreshold = 499;

    public static TheoryData<string> PhaseP2PipelineGenerationOutliers =>
    [
        "src/Api/InfraFlowSculptor.PipelineGeneration/PipelineGenerationEngine.cs",
        "src/Api/InfraFlowSculptor.PipelineGeneration/Generators/App/AppPipelineTemplatesGenerator.cs",
    ];

    [Theory]
    [MemberData(nameof(PhaseP2PipelineGenerationOutliers))]
    public void Given_PhaseP2PipelineOutlier_When_MeasuringSourceFile_Then_StaysBelowThreshold(string relativePath)
    {
        // Arrange
        var sourceFilePath = Path.Combine(ResolveRepositoryRoot(), relativePath.Replace('/', Path.DirectorySeparatorChar));

        // Act
        var lineCount = File.ReadLines(sourceFilePath).Count();

        // Assert
        lineCount.Should().BeLessThanOrEqualTo(
            PhaseP2LineThreshold,
            "Phase P2 GEN-002 requires pipeline generation outliers to be decomposed below the audit threshold: {0}",
            relativePath);
    }

    private static string ResolveRepositoryRoot()
    {
        var currentDirectory = new DirectoryInfo(AppContext.BaseDirectory);

        while (currentDirectory is not null)
        {
            if (currentDirectory.GetFiles(TestProjectMarker, SearchOption.TopDirectoryOnly).Length > 0)
            {
                return currentDirectory.Parent?.Parent?.FullName
                    ?? throw new DirectoryNotFoundException($"Could not resolve repository root from '{currentDirectory.FullName}'.");
            }

            currentDirectory = currentDirectory.Parent;
        }

        throw new DirectoryNotFoundException(
            $"Could not locate test project directory containing '{TestProjectMarker}' from '{AppContext.BaseDirectory}'.");
    }
}
