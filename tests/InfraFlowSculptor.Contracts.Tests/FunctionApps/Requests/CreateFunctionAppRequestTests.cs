using FluentAssertions;
using InfraFlowSculptor.Contracts.FunctionApps.Requests;
using InfraFlowSculptor.Contracts.Tests.TestSupport;

namespace InfraFlowSculptor.Contracts.Tests.FunctionApps.Requests;

public sealed class CreateFunctionAppRequestTests
{
    private const string ValidLocation = "WestEurope";
    private const string ValidRuntimeStack = "DotNet";
    private const string ValidRuntimeVersion = "8.0";
    private const string ValidDeploymentMode = "Code";
    private static readonly Guid ValidResourceGroupId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid ValidAppServicePlanId = Guid.Parse("22222222-2222-2222-2222-222222222222");

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
    public void Given_NullName_When_Validate_Then_Error()
    {
        // Arrange
        var sut = new CreateFunctionAppRequest
        {
            Name = null!,
            Location = ValidLocation,
            ResourceGroupId = ValidResourceGroupId,
            AppServicePlanId = ValidAppServicePlanId,
            RuntimeStack = ValidRuntimeStack,
            RuntimeVersion = ValidRuntimeVersion,
            DeploymentMode = ValidDeploymentMode,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(CreateFunctionAppRequest.Name)).Should().BeTrue();
    }

    [Fact]
    public void Given_NullLocation_When_Validate_Then_Error()
    {
        // Arrange
        var sut = new CreateFunctionAppRequest
        {
            Name = "func-prod",
            Location = null!,
            ResourceGroupId = ValidResourceGroupId,
            AppServicePlanId = ValidAppServicePlanId,
            RuntimeStack = ValidRuntimeStack,
            RuntimeVersion = ValidRuntimeVersion,
            DeploymentMode = ValidDeploymentMode,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(CreateFunctionAppRequest.Location)).Should().BeTrue();
    }

    [Fact]
    public void Given_InvalidLocation_When_Validate_Then_Error()
    {
        // Arrange
        var sut = CreateRequest(location: "InvalidRegion");

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(CreateFunctionAppRequest.Location)).Should().BeTrue();
    }

    [Fact]
    public void Given_EmptyResourceGroupId_When_Validate_Then_Error()
    {
        // Arrange
        var sut = CreateRequest(resourceGroupId: Guid.Empty);

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(CreateFunctionAppRequest.ResourceGroupId)).Should().BeTrue();
    }

    [Fact]
    public void Given_EmptyAppServicePlanId_When_Validate_Then_Error()
    {
        // Arrange
        var sut = CreateRequest(appServicePlanId: Guid.Empty);

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(CreateFunctionAppRequest.AppServicePlanId)).Should().BeTrue();
    }

    [Fact]
    public void Given_InvalidRuntimeStack_When_Validate_Then_Error()
    {
        // Arrange
        var sut = CreateRequest(runtimeStack: "Cobol");

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(CreateFunctionAppRequest.RuntimeStack)).Should().BeTrue();
    }

    [Fact]
    public void Given_InvalidDeploymentMode_When_Validate_Then_Error()
    {
        // Arrange
        var sut = CreateRequest(deploymentMode: "Magic");

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(CreateFunctionAppRequest.DeploymentMode)).Should().BeTrue();
    }

    [Fact]
    public void Given_TraversingDockerfilePath_When_Validate_Then_Error()
    {
        // Arrange
        var sut = CreateRequest(dockerfilePath: "../src/Func/Dockerfile");

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(CreateFunctionAppRequest.DockerfilePath)).Should().BeTrue();
    }

    [Fact]
    public void Given_TraversingSourceCodePath_When_Validate_Then_Error()
    {
        // Arrange
        var sut = CreateRequest(sourceCodePath: "../src/Func");

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(CreateFunctionAppRequest.SourceCodePath)).Should().BeTrue();
    }

    private static CreateFunctionAppRequest CreateValidRequest() => CreateRequest();

    private static CreateFunctionAppRequest CreateRequest(
        string? name = null,
        string? location = null,
        Guid? resourceGroupId = null,
        Guid? appServicePlanId = null,
        string? runtimeStack = null,
        string? runtimeVersion = null,
        string? deploymentMode = null,
        string? dockerfilePath = null,
        string? sourceCodePath = null)
    {
        return new CreateFunctionAppRequest
        {
            Name = name ?? "func-prod",
            Location = location ?? ValidLocation,
            ResourceGroupId = resourceGroupId ?? ValidResourceGroupId,
            AppServicePlanId = appServicePlanId ?? ValidAppServicePlanId,
            RuntimeStack = runtimeStack ?? ValidRuntimeStack,
            RuntimeVersion = runtimeVersion ?? ValidRuntimeVersion,
            DeploymentMode = deploymentMode ?? ValidDeploymentMode,
            DockerfilePath = dockerfilePath,
            SourceCodePath = sourceCodePath,
        };
    }
}
