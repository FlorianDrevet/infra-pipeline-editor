using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.PipelineGeneration.Tests.GenerationCore;

public sealed class AppPipelineModeTests
{
    [Fact]
    public void Given_AppPipelineMode_When_DefaultLayout_Then_IsolatedIsZeroAndCombinedIsOne()
    {
        // Act / Assert
        ((int)AppPipelineMode.Isolated).Should().Be(0);
        ((int)AppPipelineMode.Combined).Should().Be(1);
    }

    [Fact]
    public void Given_AppPipelineMode_When_GetNames_Then_ReturnsIsolatedAndCombined()
    {
        // Act
        var names = Enum.GetNames<AppPipelineMode>();

        // Assert
        names.Should().BeEquivalentTo(["Isolated", "Combined"]);
    }
}
