using FluentAssertions;
using InfraFlowSculptor.BicepGeneration.Pipeline;
using NSubstitute;

namespace InfraFlowSculptor.BicepGeneration.Tests.Pipeline;

public sealed class BicepGenerationPipelineTests
{
    [Fact]
    public void Given_StagesInRandomOrder_When_Execute_Then_RunsStagesInAscendingOrder()
    {
        // Arrange
        var executionOrder = new List<int>();

        var stage300 = Substitute.For<IBicepGenerationStage>();
        stage300.Order.Returns(300);
        stage300.When(s => s.Execute(Arg.Any<BicepGenerationContext>()))
            .Do(_ => executionOrder.Add(300));

        var stage100 = Substitute.For<IBicepGenerationStage>();
        stage100.Order.Returns(100);
        stage100.When(s => s.Execute(Arg.Any<BicepGenerationContext>()))
            .Do(_ => executionOrder.Add(100));

        var stage200 = Substitute.For<IBicepGenerationStage>();
        stage200.Order.Returns(200);
        stage200.When(s => s.Execute(Arg.Any<BicepGenerationContext>()))
            .Do(_ => executionOrder.Add(200));

        var sut = new BicepGenerationPipeline([stage300, stage100, stage200]);
        var context = new BicepGenerationContext
        {
            Request = new GenerationCore.Models.GenerationRequest(),
        };

        // Act
        sut.Execute(context);

        // Assert
        executionOrder.Should().ContainInOrder(100, 200, 300);
    }

    [Fact]
    public void Given_NoStages_When_Execute_Then_CompletesWithoutError()
    {
        // Arrange
        var sut = new BicepGenerationPipeline([]);
        var context = new BicepGenerationContext
        {
            Request = new GenerationCore.Models.GenerationRequest(),
        };

        // Act
        var act = () => sut.Execute(context);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Given_SingleStage_When_Execute_Then_StageReceivesContext()
    {
        // Arrange
        var stage = Substitute.For<IBicepGenerationStage>();
        stage.Order.Returns(100);

        var sut = new BicepGenerationPipeline([stage]);
        var context = new BicepGenerationContext
        {
            Request = new GenerationCore.Models.GenerationRequest(),
        };

        // Act
        sut.Execute(context);

        // Assert
        stage.Received(1).Execute(context);
    }

    [Fact]
    public void Given_MultipleStagesWithSameOrder_When_Execute_Then_AllStagesRun()
    {
        // Arrange
        var executionCount = 0;

        var stageA = Substitute.For<IBicepGenerationStage>();
        stageA.Order.Returns(100);
        stageA.When(s => s.Execute(Arg.Any<BicepGenerationContext>()))
            .Do(_ => Interlocked.Increment(ref executionCount));

        var stageB = Substitute.For<IBicepGenerationStage>();
        stageB.Order.Returns(100);
        stageB.When(s => s.Execute(Arg.Any<BicepGenerationContext>()))
            .Do(_ => Interlocked.Increment(ref executionCount));

        var sut = new BicepGenerationPipeline([stageA, stageB]);
        var context = new BicepGenerationContext
        {
            Request = new GenerationCore.Models.GenerationRequest(),
        };

        // Act
        sut.Execute(context);

        // Assert
        executionCount.Should().Be(2);
    }
}
