using InfraFlowSculptor.PipelineGeneration.Bootstrap;
using InfraFlowSculptor.PipelineGeneration.Bootstrap.Stages;
using InfraFlowSculptor.PipelineGeneration.Models;

namespace InfraFlowSculptor.PipelineGeneration.Tests.Bootstrap;

/// <summary>
/// Unit tests for <see cref="ServiceConnectionValidationJobStage"/>.
/// </summary>
public sealed class ServiceConnectionValidationJobStageTests
{
    [Fact]
    public void Given_NoServiceConnections_When_Execute_Then_NothingEmitted()
    {
        // Arrange
        var stage = new ServiceConnectionValidationJobStage();
        var context = CreateContext(serviceConnections: []);

        // Act
        stage.Execute(context);

        // Assert
        context.Builder.ToString().Should().BeEmpty();
        context.HasProvisioningJob.Should().BeFalse();
    }

    [Fact]
    public void Given_ServiceConnections_When_Execute_Then_EmitsValidationJob()
    {
        // Arrange
        var stage = new ServiceConnectionValidationJobStage();
        var context = CreateContext(serviceConnections:
        [
            new BootstrapServiceConnectionDefinition("arm-dev", BootstrapServiceConnectionTypes.AzureRM, "dev"),
            new BootstrapServiceConnectionDefinition("acr-docker-dev", BootstrapServiceConnectionTypes.DockerRegistry, "dev"),
        ]);

        // Act
        stage.Execute(context);

        // Assert
        var yaml = context.Builder.ToString();
        yaml.Should().Contain("ValidateServiceConnections");
        yaml.Should().Contain("Validate Service Connections");
        yaml.Should().Contain("arm-dev");
        yaml.Should().Contain("acr-docker-dev");
        yaml.Should().Contain("_apis/serviceendpoint/endpoints");
        yaml.Should().Contain("##[error]Missing service connections:");
        yaml.Should().Contain("SYSTEM_ACCESSTOKEN: $(System.AccessToken)");
        context.HasProvisioningJob.Should().BeTrue();
    }

    [Fact]
    public void Given_DuplicateServiceConnections_When_Execute_Then_EmitsEachOnlyOnce()
    {
        // Arrange
        var stage = new ServiceConnectionValidationJobStage();
        var context = CreateContext(serviceConnections:
        [
            new BootstrapServiceConnectionDefinition("shared-arm", BootstrapServiceConnectionTypes.AzureRM, "dev"),
            new BootstrapServiceConnectionDefinition("shared-arm", BootstrapServiceConnectionTypes.AzureRM, "prod"),
        ]);

        // Act
        stage.Execute(context);

        // Assert
        var yaml = context.Builder.ToString();
        var occurrences = yaml.Split("shared-arm").Length - 1;

        // Should appear in the check + success message + error append = once per distinct SC, not per duplicate
        // The distinct-by is by name, so we only get one validation block
        yaml.Should().Contain("shared-arm");
        occurrences.Should().BeLessThanOrEqualTo(5, "duplicate SC names should be deduplicated");
    }

    [Fact]
    public void Given_ServiceConnections_When_Execute_Then_OrderIs250()
    {
        var stage = new ServiceConnectionValidationJobStage();
        stage.Order.Should().Be(250);
    }

    [Fact]
    public void Given_ServiceConnectionWithSingleQuote_When_Execute_Then_EscapesProperly()
    {
        // Arrange
        var stage = new ServiceConnectionValidationJobStage();
        var context = CreateContext(serviceConnections:
        [
            new BootstrapServiceConnectionDefinition("O'Brien-ARM", BootstrapServiceConnectionTypes.AzureRM, "dev"),
        ]);

        // Act
        stage.Execute(context);

        // Assert
        var yaml = context.Builder.ToString();
        yaml.Should().Contain("O''Brien-ARM", "single quotes must be escaped for PowerShell");
    }

    private static BootstrapPipelineContext CreateContext(
        IReadOnlyList<BootstrapServiceConnectionDefinition> serviceConnections)
    {
        return new BootstrapPipelineContext
        {
            Request = new BootstrapGenerationRequest
            {
                OrganizationName = "contoso",
                ProjectName = "ifs",
                RepositoryName = "ifs",
                DefaultBranch = "main",
                ServiceConnections = serviceConnections,
            },
        };
    }
}
