using InfraFlowSculptor.PipelineGeneration.Bootstrap;
using InfraFlowSculptor.PipelineGeneration.Bootstrap.Stages;
using InfraFlowSculptor.PipelineGeneration.Models;

namespace InfraFlowSculptor.PipelineGeneration.Tests.Bootstrap.Stages;

/// <summary>
/// Unit tests for <see cref="NoOpFallbackStage"/>.
/// </summary>
public sealed class NoOpFallbackStageTests
{
    private readonly NoOpFallbackStage _sut = new();

    [Fact]
    public void Given_NoProvisioningJobDone_When_Execute_Then_EmitsNoOpJob()
    {
        // Arrange
        var context = new BootstrapPipelineContext
        {
            Request = new BootstrapGenerationRequest
            {
                OrganizationName = "contoso",
                ProjectName = "ifs",
                RepositoryName = "ifs",
                DefaultBranch = "main",
            },
        };

        // Act
        _sut.Execute(context);

        // Assert
        var output = context.Builder.ToString();
        output.Should().Contain("NothingToProvision");
        output.Should().Contain("No pipelines, environments, or variable groups were requested for bootstrap.");
    }

    [Fact]
    public void Given_ProvisioningJobAlreadyDone_When_Execute_Then_NoOutput()
    {
        // Arrange
        var context = new BootstrapPipelineContext
        {
            Request = new BootstrapGenerationRequest
            {
                OrganizationName = "contoso",
                ProjectName = "ifs",
                RepositoryName = "ifs",
                DefaultBranch = "main",
            },
            HasProvisioningJob = true,
        };

        // Act
        _sut.Execute(context);

        // Assert
        context.Builder.ToString().Should().BeEmpty();
    }

    [Fact]
    public void Order_Is_999()
    {
        _sut.Order.Should().Be(999);
    }
}
