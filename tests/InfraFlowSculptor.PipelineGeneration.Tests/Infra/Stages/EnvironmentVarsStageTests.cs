using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.GenerationCore.Models;
using InfraFlowSculptor.PipelineGeneration.Infra;
using InfraFlowSculptor.PipelineGeneration.Infra.Stages;

namespace InfraFlowSculptor.PipelineGeneration.Tests.Infra.Stages;

/// <summary>
/// Unit tests for <see cref="EnvironmentVarsStage"/>.
/// </summary>
public sealed class EnvironmentVarsStageTests
{
    private readonly EnvironmentVarsStage _sut = new();

    [Fact]
    public void Order_Is_500()
    {
        _sut.Order.Should().Be(500);
    }

    [Fact]
    public void Given_StandardContext_When_Execute_Then_AddsPerEnvironmentFiles()
    {
        // Arrange
        var context = CreateContext(isMonoRepo: false);

        // Act
        _sut.Execute(context);

        // Assert
        context.Files.Should().ContainKey("variables/dev.yml");
        context.Files.Should().ContainKey("variables/prd.yml");
    }

    [Fact]
    public void Given_MonoRepoContext_When_Execute_Then_NoOutput()
    {
        // Arrange
        var context = CreateContext(isMonoRepo: true);

        // Act
        _sut.Execute(context);

        // Assert
        context.Files.Should().BeEmpty();
    }

    [Fact]
    public void Given_ContextWithSubscriptionAndLocation_When_Execute_Then_FileContainsBoth()
    {
        // Arrange
        var context = CreateContext(isMonoRepo: false);

        // Act
        _sut.Execute(context);

        // Assert
        var devFile = context.Files["variables/dev.yml"];
        devFile.Should().Contain("subscriptionId: 'sub-dev'");
        devFile.Should().Contain($"location: '{Location.DefaultAzureRegionKey}'");
    }

    private static InfraPipelineContext CreateContext(bool isMonoRepo) => new()
    {
        Request = new GenerationRequest
        {
            Environments =
            [
                new EnvironmentDefinition { Name = "Development", ShortName = "dev", Location = Location.DefaultAzureRegionKey, SubscriptionId = "sub-dev" },
                new EnvironmentDefinition { Name = "Production", ShortName = "prd", Location = Location.ToAzureRegionKey(Location.LocationEnum.NorthEurope), SubscriptionId = "sub-prd" },
            ],
        },
        ConfigName = "my-config",
        IsMonoRepo = isMonoRepo,
    };
}
