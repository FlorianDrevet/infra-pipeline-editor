using FluentAssertions;
using InfraFlowSculptor.Contracts.Tests.TestSupport;
using InfraFlowSculptor.Contracts.WebApps.Requests;

namespace InfraFlowSculptor.Contracts.Tests.WebApps.Requests;

public sealed class CreateWebAppRequestTests
{
    private const string ValidLocation = "WestEurope";
    private const string ValidRuntimeStack = "DotNet";
    private const string ValidDeploymentMode = "Code";

    [Fact]
    public void Given_ValidRequest_When_Validate_Then_NoError()
    {
        // Arrange
        var sut = CreateValidRequest();

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void Given_EmptyResourceGroupId_When_Validate_Then_ReturnsGuidError()
    {
        // Arrange
        var sut = CreateRequest(resourceGroupId: Guid.Empty);

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(CreateWebAppRequest.ResourceGroupId)).Should().BeTrue();
    }

    [Fact]
    public void Given_EmptyAppServicePlanId_When_Validate_Then_ReturnsGuidError()
    {
        // Arrange
        var sut = CreateRequest(appServicePlanId: Guid.Empty);

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(CreateWebAppRequest.AppServicePlanId)).Should().BeTrue();
    }

    [Fact]
    public void Given_InvalidRuntimeStack_When_Validate_Then_ReturnsEnumError()
    {
        // Arrange
        var sut = CreateRequest(runtimeStack: "Cobol");

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(CreateWebAppRequest.RuntimeStack)).Should().BeTrue();
    }

    [Fact]
    public void Given_InvalidDeploymentMode_When_Validate_Then_ReturnsEnumError()
    {
        // Arrange
        var sut = CreateRequest(deploymentMode: "Magic");

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(CreateWebAppRequest.DeploymentMode)).Should().BeTrue();
    }

    [Fact]
    public void Given_DockerfilePathExceedingMaxLength_When_Validate_Then_ReturnsLengthError()
    {
        // Arrange
        var sut = CreateRequest(dockerfilePath: new string('p', 501));

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(CreateWebAppRequest.DockerfilePath)).Should().BeTrue();
    }

    [Fact]
    public void Given_BuildCommandExceedingMaxLength_When_Validate_Then_ReturnsLengthError()
    {
        // Arrange
        var sut = CreateRequest(buildCommand: new string('c', 1001));

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(CreateWebAppRequest.BuildCommand)).Should().BeTrue();
    }

    private static CreateWebAppRequest CreateValidRequest() => CreateRequest();

    private static CreateWebAppRequest CreateRequest(
        Guid? resourceGroupId = null,
        Guid? appServicePlanId = null,
        string? runtimeStack = null,
        string? deploymentMode = null,
        string? dockerfilePath = null,
        string? buildCommand = null)
    {
        return new CreateWebAppRequest
        {
            ResourceGroupId = resourceGroupId ?? Guid.NewGuid(),
            Name = "web-prod",
            Location = ValidLocation,
            AppServicePlanId = appServicePlanId ?? Guid.NewGuid(),
            RuntimeStack = runtimeStack ?? ValidRuntimeStack,
            RuntimeVersion = "8.0",
            DeploymentMode = deploymentMode ?? ValidDeploymentMode,
            DockerfilePath = dockerfilePath,
            BuildCommand = buildCommand,
        };
    }
}
