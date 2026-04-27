using InfraFlowSculptor.GenerationCore.Models;
using InfraFlowSculptor.PipelineGeneration.Infra;

namespace InfraFlowSculptor.PipelineGeneration.Tests.Infra;

/// <summary>
/// Unit tests for <see cref="InfraPipeline"/> orchestrator.
/// </summary>
public sealed class InfraPipelineTests
{
    [Fact]
    public void Given_OrderedStages_When_Execute_Then_StagesRunInOrder()
    {
        // Arrange
        var executionLog = new List<int>();

        var stages = new IInfraPipelineStage[]
        {
            new TrackingStage(300, executionLog),
            new TrackingStage(100, executionLog),
            new TrackingStage(200, executionLog),
        };

        var sut = new InfraPipeline(stages);
        var context = CreateMinimalContext();

        // Act
        sut.Execute(context);

        // Assert
        executionLog.Should().ContainInOrder(100, 200, 300);
    }

    [Fact]
    public void Given_NoStages_When_Execute_Then_ContextFilesEmpty()
    {
        // Arrange
        var sut = new InfraPipeline([]);
        var context = CreateMinimalContext();

        // Act
        sut.Execute(context);

        // Assert
        context.Files.Should().BeEmpty();
    }

    private static InfraPipelineContext CreateMinimalContext() => new()
    {
        Request = new GenerationRequest(),
        ConfigName = "test-config",
        IsMonoRepo = false,
    };

    /// <summary>
    /// Stub stage that records its order into a shared log when executed.
    /// </summary>
    private sealed class TrackingStage(int order, List<int> log) : IInfraPipelineStage
    {
        public int Order => order;

        public void Execute(InfraPipelineContext context)
        {
            log.Add(Order);
        }
    }
}
