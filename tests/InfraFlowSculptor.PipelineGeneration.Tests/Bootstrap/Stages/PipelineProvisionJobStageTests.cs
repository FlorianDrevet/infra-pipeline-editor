using InfraFlowSculptor.PipelineGeneration.Bootstrap;
using InfraFlowSculptor.PipelineGeneration.Bootstrap.Stages;
using InfraFlowSculptor.PipelineGeneration.Models;

namespace InfraFlowSculptor.PipelineGeneration.Tests.Bootstrap.Stages;

/// <summary>
/// Unit tests for <see cref="PipelineProvisionJobStage"/>.
/// </summary>
public sealed class PipelineProvisionJobStageTests
{
    private readonly PipelineProvisionJobStage _sut = new();

    [Fact]
    public void Given_NoPipelines_When_Execute_Then_NoOutput()
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
        context.Builder.ToString().Should().BeEmpty();
        context.HasProvisioningJob.Should().BeFalse();
    }

    [Fact]
    public void Given_PipelinesPresent_When_Execute_Then_AppendsPipelineProvisionJob()
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
                Pipelines =
                [
                    new BootstrapPipelineDefinition("CI", ".azuredevops/ci.yml", "\\App"),
                ],
            },
        };

        // Act
        _sut.Execute(context);

        // Assert
        var output = context.Builder.ToString();
        output.Should().Contain("ProvisionPipelineDefinitions");
        output.Should().Contain("Create Pipeline: CI");
        context.HasProvisioningJob.Should().BeTrue();
    }

    [Fact]
    public void Given_ApplicationOnlyWithSharedResources_When_Execute_Then_DependsOnValidateSharedResources()
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
                Pipelines =
                [
                    new BootstrapPipelineDefinition("CI", ".azuredevops/ci.yml", "\\App"),
                ],
                Environments = [new BootstrapEnvironmentDefinition("dev", "Dev", false)],
            },
        };

        // Act
        _sut.Execute(context);

        // Assert
        context.Builder.ToString().Should().Contain("dependsOn: ValidateSharedResources");
    }

    [Fact]
    public void Given_FullOwnerWithPipelines_When_Execute_Then_NoDependsOn()
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
                Pipelines =
                [
                    new BootstrapPipelineDefinition("CI", ".azuredevops/ci.yml", "\\App"),
                ],
            },
        };

        // Act
        _sut.Execute(context);

        // Assert
        context.Builder.ToString().Should().NotContain("dependsOn:");
    }

    [Fact]
    public void Order_Is_300()
    {
        _sut.Order.Should().Be(300);
    }
}
