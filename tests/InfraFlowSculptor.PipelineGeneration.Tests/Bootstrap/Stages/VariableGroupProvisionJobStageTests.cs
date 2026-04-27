using InfraFlowSculptor.PipelineGeneration.Bootstrap;
using InfraFlowSculptor.PipelineGeneration.Bootstrap.Stages;
using InfraFlowSculptor.PipelineGeneration.Models;

namespace InfraFlowSculptor.PipelineGeneration.Tests.Bootstrap.Stages;

/// <summary>
/// Unit tests for <see cref="VariableGroupProvisionJobStage"/>.
/// </summary>
public sealed class VariableGroupProvisionJobStageTests
{
    private readonly VariableGroupProvisionJobStage _sut = new();

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
                VariableGroups =
                [
                    new BootstrapVariableGroupDefinition("vg", [new BootstrapVariable("KEY", "VAL", false)]),
                ],
            },
        };

        // Act
        _sut.Execute(context);

        // Assert
        context.Builder.ToString().Should().BeEmpty();
        context.HasProvisioningJob.Should().BeFalse();
    }

    [Fact]
    public void Given_FullOwnerNoVariableGroups_When_Execute_Then_NoOutput()
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
    public void Given_FullOwnerWithVariableGroups_When_Execute_Then_AppendsVariableGroupJob()
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
                VariableGroups =
                [
                    new BootstrapVariableGroupDefinition("my-vars", [new BootstrapVariable("KEY", "VAL", false)]),
                ],
            },
        };

        // Act
        _sut.Execute(context);

        // Assert
        var output = context.Builder.ToString();
        output.Should().Contain("ProvisionVariableGroups");
        output.Should().Contain("Create Variable Group: my-vars");
        context.HasProvisioningJob.Should().BeTrue();
    }

    [Fact]
    public void Order_Is_500()
    {
        _sut.Order.Should().Be(500);
    }
}
