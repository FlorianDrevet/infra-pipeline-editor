using InfraFlowSculptor.PipelineGeneration.Bootstrap;
using InfraFlowSculptor.PipelineGeneration.Bootstrap.Stages;
using InfraFlowSculptor.PipelineGeneration.Models;

namespace InfraFlowSculptor.PipelineGeneration.Tests.Bootstrap.Stages;

/// <summary>
/// Unit tests for <see cref="ValidateSharedResourcesJobStage"/>.
/// </summary>
public sealed class ValidateSharedResourcesJobStageTests
{
    private readonly ValidateSharedResourcesJobStage _sut = new();

    [Fact]
    public void Given_FullOwnerMode_When_Execute_Then_NoOutput()
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
        context.Builder.ToString().Should().BeEmpty();
        context.HasProvisioningJob.Should().BeFalse();
    }

    [Fact]
    public void Given_ApplicationOnlyNoSharedResources_When_Execute_Then_NoOutput()
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
            },
        };

        // Act
        _sut.Execute(context);

        // Assert
        context.Builder.ToString().Should().BeEmpty();
        context.HasProvisioningJob.Should().BeFalse();
    }

    [Fact]
    public void Given_ApplicationOnlyWithEnvironments_When_Execute_Then_AppendsValidationJob()
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
        var output = context.Builder.ToString();
        output.Should().Contain("ValidateSharedResources");
        output.Should().Contain("Validate shared project resources");
        context.HasProvisioningJob.Should().BeTrue();
    }

    [Fact]
    public void Order_Is_200()
    {
        _sut.Order.Should().Be(200);
    }
}
