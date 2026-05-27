using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Models;
using InfraFlowSculptor.PipelineGeneration.Generators;
using InfraFlowSculptor.PipelineGeneration.Generators.App;
using InfraFlowSculptor.PipelineGeneration.Tests.Fixtures;

namespace InfraFlowSculptor.PipelineGeneration.Tests;

public sealed class AppPipelineContainerRegistryServiceConnectionTests
{
    private readonly AppPipelineGenerationEngine _sut = new(new IAppPipelineGenerator[]
    {
        new ContainerAppPipelineGenerator(),
    });

    [Fact]
    public void Given_ContainerAppWithEnvironmentAcrServiceConnection_When_Generate_Then_CiWrapperPassesConnectionToSharedTemplate()
    {
        // Arrange
        const string serviceConnection = "ifs-dev-acr-sc";
        var request = AppPipelineRequestFixtures.ContainerApp();
        request.ContainerRegistryServiceConnections =
        [
            new ContainerRegistryServiceConnectionDefinition("dev", serviceConnection),
        ];

        // Act
        var result = _sut.Generate(request);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Files["ci.app-pipeline.yml"].Should().Contain($"containerRegistryServiceConnection: '{serviceConnection}'");
    }

    [Fact]
    public void Given_SharedTemplates_When_GenerateSharedTemplates_Then_ContainerCiTemplatesPassServiceConnectionToAcrLoginStep()
    {
        // Act
        var files = AppPipelineGenerationEngine.GenerateSharedTemplates();

        // Assert
        files[".azuredevops/pipelines/app-ci-container.pipeline.yml"].Should()
            .Contain("- name: containerRegistryServiceConnection")
            .And.Contain("containerRegistryServiceConnection: ${{ parameters.containerRegistryServiceConnection }}");
        files[".azuredevops/jobs/app-ci-container.job.yml"].Should()
            .Contain("- name: containerRegistryServiceConnection")
            .And.Contain("containerRegistryServiceConnection: ${{ parameters.containerRegistryServiceConnection }}");
        files[".azuredevops/steps/app-acr-login.step.yml"].Should()
            .Contain("default: '$(containerRegistryServiceConnection)'")
            .And.Contain("containerRegistry: ${{ parameters.containerRegistryServiceConnection }}");
    }
}