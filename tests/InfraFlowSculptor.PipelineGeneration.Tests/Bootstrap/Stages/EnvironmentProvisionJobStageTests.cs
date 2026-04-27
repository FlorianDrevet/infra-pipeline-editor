using InfraFlowSculptor.PipelineGeneration.Bootstrap;
using InfraFlowSculptor.PipelineGeneration.Bootstrap.Stages;
using InfraFlowSculptor.PipelineGeneration.Models;

namespace InfraFlowSculptor.PipelineGeneration.Tests.Bootstrap.Stages;

/// <summary>
/// Unit tests for <see cref="EnvironmentProvisionJobStage"/>.
/// </summary>
public sealed class EnvironmentProvisionJobStageTests
{
    private readonly EnvironmentProvisionJobStage _sut = new();

    [Fact]
    public void Given_ApplicationOnlyMode_When_Execute_Then_NoOutput()
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
                Mode = BootstrapMode.ApplicationOnly,
                Environments = [new BootstrapEnvironmentDefinition("dev", "Dev", false)],
            },
        };

        // Act
        _sut.Execute(context);

        // Assert
        context.Builder.ToString().Should().BeEmpty();
        context.HasProvisioningJob.Should().BeFalse();
    }

    [Fact]
    public void Given_FullOwnerNoEnvironments_When_Execute_Then_NoOutput()
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
                Mode = BootstrapMode.FullOwner,
            },
        };

        // Act
        _sut.Execute(context);

        // Assert
        context.Builder.ToString().Should().BeEmpty();
        context.HasProvisioningJob.Should().BeFalse();
    }

    [Fact]
    public void Given_FullOwnerWithEnvironments_When_Execute_Then_AppendsEnvironmentJob()
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
                Mode = BootstrapMode.FullOwner,
                Environments = [new BootstrapEnvironmentDefinition("dev", "Dev", false)],
            },
        };

        // Act
        _sut.Execute(context);

        // Assert
        var output = context.Builder.ToString();
        output.Should().Contain("ProvisionEnvironments");
        output.Should().Contain("Create Environment: dev");
        context.HasProvisioningJob.Should().BeTrue();
    }

    [Fact]
    public void Order_Is_400()
    {
        _sut.Order.Should().Be(400);
    }
}
