using FluentAssertions;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.BicepGeneration.Pipeline;
using InfraFlowSculptor.BicepGeneration.Pipeline.Stages;
using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.BicepGeneration.Tests.Pipeline.Stages;

public sealed class AssemblyStageTests
{
    private readonly AssemblyStage _sut = new();

    [Fact]
    public void Given_Stage_When_CheckOrder_Then_Returns900()
    {
        _sut.Order.Should().Be(900);
    }

    [Fact]
    public void Given_EmptyWorkItems_When_Execute_Then_ResultIsPopulated()
    {
        // Arrange
        var context = new BicepGenerationContext
        {
            Request = new GenerationRequest
            {
                Resources = [],
                ResourceGroups = [],
                Environments = [],
                EnvironmentNames = [],
                RoleAssignments = [],
                AppSettings = [],
                ExistingResourceReferences = [],
                NamingContext = new NamingContext(),
                ProjectTags = new Dictionary<string, string>(),
                ConfigTags = new Dictionary<string, string>(),
            },
        };

        // Act
        _sut.Execute(context);

        // Assert — Result should be non-null (BicepAssembler runs and returns a GenerationResult)
        context.Result.Should().NotBeNull();
    }

    [Fact]
    public void Given_NullResultBefore_When_Execute_Then_ResultPopulatedByAssembler()
    {
        // Arrange
        var context = new BicepGenerationContext
        {
            Request = new GenerationRequest
            {
                Resources = [],
                ResourceGroups = [],
                Environments = [],
                EnvironmentNames = [],
                RoleAssignments = [],
                AppSettings = [],
                ExistingResourceReferences = [],
                NamingContext = new NamingContext(),
                ProjectTags = new Dictionary<string, string>(),
                ConfigTags = new Dictionary<string, string>(),
            },
        };
        context.Result.Should().BeNull("pre-condition: Result is null before assembly");

        // Act
        _sut.Execute(context);

        // Assert
        context.Result.Should().NotBeNull();
    }
}
