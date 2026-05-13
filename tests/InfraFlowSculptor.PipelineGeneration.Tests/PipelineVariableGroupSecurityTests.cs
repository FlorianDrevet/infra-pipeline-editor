using ErrorOr;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Models;
using InfraFlowSculptor.PipelineGeneration.Infra;
using InfraFlowSculptor.PipelineGeneration.Generators;
using InfraFlowSculptor.PipelineGeneration.Generators.App;
using InfraFlowSculptor.PipelineGeneration.Tests.Fixtures;

namespace InfraFlowSculptor.PipelineGeneration.Tests;

public sealed class PipelineVariableGroupSecurityTests
{
    [Fact]
    public void Given_GroupNameWithAzureTemplateExpression_When_GenerateInfraPipeline_Then_ReturnsValidationError()
    {
        // Arrange
        var sut = new PipelineGenerationEngine();
        var request = GenerationRequestFixtures.StandardStandalone();
        request.PipelineVariableGroups =
        [
            new PipelineVariableGroupDefinition
            {
                GroupName = "ifs-${{ variables.hijack }}",
            },
        ];

        // Act
        var result = sut.Generate(request, "core");

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
        result.FirstError.Code.Should().Be("Generation.InvalidPipelineVariableGroupName");
        result.FirstError.Description.Should().Contain("must not contain Azure DevOps template expressions");
    }

    [Fact]
    public void Given_GroupNameWithAzureTemplateExpression_When_GenerateAppPipeline_Then_ReturnsValidationError()
    {
        // Arrange
        var sut = new AppPipelineGenerationEngine([new WebAppCodePipelineGenerator()]);
        var request = AppPipelineRequestFixtures.WebAppCode();
        request.PipelineVariableGroups =
        [
            new PipelineVariableGroupDefinition
            {
                GroupName = "ifs-${{ variables.hijack }}",
            },
        ];

        // Act
    var result = sut.Generate(request);

        // Assert
    result.IsError.Should().BeTrue();
    result.FirstError.Type.Should().Be(ErrorType.Validation);
    result.FirstError.Code.Should().Be("Generation.InvalidPipelineVariableGroupName");
    result.FirstError.Description.Should().Contain("must not contain Azure DevOps template expressions");
    }

    [Fact]
    public void Given_GroupNameWithEnvPlaceholder_When_GenerateInfraPipeline_Then_EmitsQuotedSupportedPlaceholder()
    {
        // Arrange
        var sut = new PipelineGenerationEngine();
        var request = GenerationRequestFixtures.StandardStandalone();
        request.PipelineVariableGroups =
        [
            new PipelineVariableGroupDefinition
            {
                GroupName = "ifs-shared-{env}",
            },
        ];

        // Act
        var result = sut.Generate(request, "core");

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.TemplateFiles["release.pipeline.yml"]
            .Should()
            .Contain("- group: 'ifs-shared-${{ environment }}'");
    }

    [Fact]
    public void Given_UnexpectedInvalidOperationException_When_GenerateInfraPipeline_Then_RethrowsInsteadOfReturningValidationError()
    {
        // Arrange
        var sut = new PipelineGenerationEngine(new InfraPipeline([new ThrowingInfraPipelineStage("Unexpected pipeline failure")])) ;
        var request = GenerationRequestFixtures.StandardStandalone();

        // Act
        var act = () => sut.Generate(request, "core");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Unexpected pipeline failure");
    }

    private sealed class ThrowingInfraPipelineStage : IInfraPipelineStage
    {
        private readonly string _message;

        public ThrowingInfraPipelineStage(string message)
        {
            _message = message;
        }

        public int Order => 0;

        public void Execute(InfraPipelineContext context)
        {
            throw new InvalidOperationException(_message);
        }
    }
}