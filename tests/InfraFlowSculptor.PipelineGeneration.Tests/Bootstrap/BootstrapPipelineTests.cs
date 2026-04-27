using InfraFlowSculptor.PipelineGeneration.Bootstrap;

namespace InfraFlowSculptor.PipelineGeneration.Tests.Bootstrap;

/// <summary>
/// Unit tests for <see cref="BootstrapPipeline"/> orchestrator.
/// </summary>
public sealed class BootstrapPipelineTests
{
    [Fact]
    public void Given_OrderedStages_When_Execute_Then_StagesRunInOrder()
    {
        // Arrange
        var executionLog = new List<int>();

        var stages = new IBootstrapPipelineStage[]
        {
            new TrackingStage(300, executionLog),
            new TrackingStage(100, executionLog),
            new TrackingStage(200, executionLog),
        };

        var sut = new BootstrapPipeline(stages);
        var context = new BootstrapPipelineContext
        {
            Request = new Models.BootstrapGenerationRequest
            {
                OrganizationName = "contoso",
                ProjectName = "ifs",
                RepositoryName = "ifs",
                DefaultBranch = "main",
            },
        };

        // Act
        sut.Execute(context);

        // Assert
        executionLog.Should().ContainInOrder(100, 200, 300);
    }

    [Fact]
    public void Given_NoStages_When_Execute_Then_ContextUnchanged()
    {
        // Arrange
        var sut = new BootstrapPipeline([]);
        var context = new BootstrapPipelineContext
        {
            Request = new Models.BootstrapGenerationRequest
            {
                OrganizationName = "contoso",
                ProjectName = "ifs",
                RepositoryName = "ifs",
                DefaultBranch = "main",
            },
        };

        // Act
        sut.Execute(context);

        // Assert
        context.Builder.ToString().Should().BeEmpty();
        context.HasProvisioningJob.Should().BeFalse();
    }

    /// <summary>
    /// Stub stage that records its order into a shared log when executed.
    /// </summary>
    private sealed class TrackingStage(int order, List<int> log) : IBootstrapPipelineStage
    {
        public int Order => order;

        public void Execute(BootstrapPipelineContext context)
        {
            log.Add(Order);
        }
    }
}
