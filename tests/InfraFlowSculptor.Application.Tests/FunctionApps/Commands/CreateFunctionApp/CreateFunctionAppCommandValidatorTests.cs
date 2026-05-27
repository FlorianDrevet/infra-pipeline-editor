using FluentAssertions;
using InfraFlowSculptor.Application.FunctionApps.Commands.CreateFunctionApp;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.FunctionApps.Commands.CreateFunctionApp;

public sealed class CreateFunctionAppCommandValidatorTests
{
    private readonly CreateFunctionAppCommandValidator _sut = new();

    private static CreateFunctionAppCommand ValidCommand() => new(
        ResourceGroupId.CreateUnique(),
        new Name("my-func-app"),
        new Location(Location.LocationEnum.WestEurope),
        AppServicePlanId: Guid.NewGuid(),
        RuntimeStack: "DotNet",
        RuntimeVersion: "10-isolated",
        HttpsOnly: true,
        DeploymentMode: "Code",
        ContainerRegistryId: null,
        AcrAuthMode: null,
        AcrPullIdentityId: null,
        DockerImageName: null);

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        var result = _sut.Validate(ValidCommand());

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_NullName_When_Validate_Then_FailsOnName()
    {
        var command = ValidCommand() with { Name = null! };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateFunctionAppCommand.Name));
    }

    [Fact]
    public void Given_NullLocation_When_Validate_Then_FailsOnLocation()
    {
        var command = ValidCommand() with { Location = null! };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateFunctionAppCommand.Location));
    }

    [Fact]
    public void Given_NullResourceGroupId_When_Validate_Then_FailsOnResourceGroupId()
    {
        var command = ValidCommand() with { ResourceGroupId = null! };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateFunctionAppCommand.ResourceGroupId));
    }

    [Fact]
    public void Given_EmptyAppServicePlanId_When_Validate_Then_FailsOnAppServicePlanId()
    {
        var command = ValidCommand() with { AppServicePlanId = Guid.Empty };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateFunctionAppCommand.AppServicePlanId));
    }

    [Fact]
    public void Given_EmptyRuntimeStack_When_Validate_Then_FailsOnRuntimeStack()
    {
        var command = ValidCommand() with { RuntimeStack = "" };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateFunctionAppCommand.RuntimeStack));
    }

    [Fact]
    public void Given_InvalidRuntimeStack_When_Validate_Then_FailsOnRuntimeStack()
    {
        var command = ValidCommand() with { RuntimeStack = "InvalidStack" };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateFunctionAppCommand.RuntimeStack));
    }

    [Fact]
    public void Given_EmptyRuntimeVersion_When_Validate_Then_FailsOnRuntimeVersion()
    {
        var command = ValidCommand() with { RuntimeVersion = "" };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateFunctionAppCommand.RuntimeVersion));
    }

    [Fact]
    public void Given_InvalidRuntimeVersion_When_Validate_Then_FailsOnRuntimeVersion()
    {
        var command = ValidCommand() with { RuntimeStack = "DotNet", RuntimeVersion = "999" };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateFunctionAppCommand.RuntimeVersion));
    }
}
