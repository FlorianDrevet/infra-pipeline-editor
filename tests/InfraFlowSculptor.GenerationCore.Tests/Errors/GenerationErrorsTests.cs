using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.GenerationCore.Errors;

namespace InfraFlowSculptor.GenerationCore.Tests.Errors;

public sealed class GenerationErrorsTests
{
    [Fact]
    public void InvalidDeploymentMode_ShouldReturnValidationError_WithCodeAndDescription()
    {
        var validModes = new[] { "Code", "Container" };

        var error = GenerationErrors.InvalidDeploymentMode("Invalid", validModes);

        error.Type.Should().Be(ErrorType.Validation);
        error.Code.Should().Be("Generation.InvalidDeploymentMode");
        error.Description.Should().Contain("Invalid");
        error.Description.Should().Contain("Code");
        error.Description.Should().Contain("Container");
    }

    [Fact]
    public void InvalidDeploymentMode_GivenEmptyValidModes_ShouldStillProduceDescription()
    {
        var error = GenerationErrors.InvalidDeploymentMode("Bad", Array.Empty<string>());

        error.Type.Should().Be(ErrorType.Validation);
        error.Code.Should().Be("Generation.InvalidDeploymentMode");
        error.Description.Should().Contain("Bad");
    }

    [Fact]
    public void MissingAppPipelineGenerator_ShouldReturnValidationError_WithCodeAndDescription()
    {
        var error = GenerationErrors.MissingAppPipelineGenerator("WebApp", "Code");

        error.Type.Should().Be(ErrorType.Validation);
        error.Code.Should().Be("Generation.MissingAppPipelineGenerator");
        error.Description.Should().Contain("WebApp");
        error.Description.Should().Contain("Code");
    }

    [Fact]
    public void InvalidPipelineVariableGroupName_ShouldReturnValidationError_WithCodeAndDescription()
    {
        var description = "Variable group name contains invalid characters.";

        var error = GenerationErrors.InvalidPipelineVariableGroupName(description);

        error.Type.Should().Be(ErrorType.Validation);
        error.Code.Should().Be("Generation.InvalidPipelineVariableGroupName");
        error.Description.Should().Be(description);
    }

    [Fact]
    public void InvalidAppPipelineConfiguration_ShouldReturnValidationError_WithCodeAndDescription()
    {
        var description = "Missing required environment definition.";

        var error = GenerationErrors.InvalidAppPipelineConfiguration(description);

        error.Type.Should().Be(ErrorType.Validation);
        error.Code.Should().Be("Generation.InvalidAppPipelineConfiguration");
        error.Description.Should().Be(description);
    }

    [Fact]
    public void InvalidInfrastructurePipelineConfiguration_ShouldReturnValidationError_WithCodeAndDescription()
    {
        var description = "No resource groups defined.";

        var error = GenerationErrors.InvalidInfrastructurePipelineConfiguration(description);

        error.Type.Should().Be(ErrorType.Validation);
        error.Code.Should().Be("Generation.InvalidInfrastructurePipelineConfiguration");
        error.Description.Should().Be(description);
    }

    [Fact]
    public void UnsupportedBicepResourceType_ShouldReturnValidationError_WithCodeAndDescription()
    {
        var description = "Resource type 'CosmosDB' is not supported.";

        var error = GenerationErrors.UnsupportedBicepResourceType(description);

        error.Type.Should().Be(ErrorType.Validation);
        error.Code.Should().Be("Generation.UnsupportedBicepResourceType");
        error.Description.Should().Be(description);
    }

    [Fact]
    public void UnsupportedBicepFeature_ShouldReturnValidationError_WithCodeAndDescription()
    {
        var description = "Private endpoints are not yet supported.";

        var error = GenerationErrors.UnsupportedBicepFeature(description);

        error.Type.Should().Be(ErrorType.Validation);
        error.Code.Should().Be("Generation.UnsupportedBicepFeature");
        error.Description.Should().Be(description);
    }

    [Fact]
    public void InvalidBicepConfiguration_ShouldReturnValidationError_WithCodeAndDescription()
    {
        var description = "SKU tier is missing for the Key Vault resource.";

        var error = GenerationErrors.InvalidBicepConfiguration(description);

        error.Type.Should().Be(ErrorType.Validation);
        error.Code.Should().Be("Generation.InvalidBicepConfiguration");
        error.Description.Should().Be(description);
    }
}
