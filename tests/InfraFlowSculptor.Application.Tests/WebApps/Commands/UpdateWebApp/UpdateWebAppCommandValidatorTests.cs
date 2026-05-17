using FluentAssertions;
using InfraFlowSculptor.Application.WebApps.Commands.UpdateWebApp;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.WebApps.Commands.UpdateWebApp;

public sealed class UpdateWebAppCommandValidatorTests
{
    private readonly UpdateWebAppCommandValidator _sut = new();

    private static UpdateWebAppCommand ValidCommand() => new(
        AzureResourceId.CreateUnique(),
        new Name("my-webapp"),
        new Location(Location.LocationEnum.FranceCentral),
        AppServicePlanId: Guid.NewGuid(),
        RuntimeStack: "DotNet",
        RuntimeVersion: "10",
        AlwaysOn: true,
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
    public void Given_EmptyId_When_Validate_Then_FailsOnId()
    {
        var command = ValidCommand() with { Id = null! };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateWebAppCommand.Id));
    }

    [Fact]
    public void Given_EmptyName_When_Validate_Then_FailsOnName()
    {
        var command = ValidCommand() with { Name = null! };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateWebAppCommand.Name));
    }

    [Fact]
    public void Given_EmptyLocation_When_Validate_Then_FailsOnLocation()
    {
        var command = ValidCommand() with { Location = null! };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateWebAppCommand.Location));
    }

    [Fact]
    public void Given_EmptyAppServicePlanId_When_Validate_Then_FailsOnAppServicePlanId()
    {
        var command = ValidCommand() with { AppServicePlanId = Guid.Empty };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateWebAppCommand.AppServicePlanId));
    }

    [Fact]
    public void Given_EmptyRuntimeStack_When_Validate_Then_FailsOnRuntimeStack()
    {
        var command = ValidCommand() with { RuntimeStack = "" };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateWebAppCommand.RuntimeStack));
    }

    [Fact]
    public void Given_InvalidRuntimeStack_When_Validate_Then_FailsOnRuntimeStack()
    {
        var command = ValidCommand() with { RuntimeStack = "BadStack" };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateWebAppCommand.RuntimeStack));
    }

    [Fact]
    public void Given_EmptyRuntimeVersion_When_Validate_Then_FailsOnRuntimeVersion()
    {
        var command = ValidCommand() with { RuntimeVersion = "" };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateWebAppCommand.RuntimeVersion));
    }

    [Fact]
    public void Given_EmptyDeploymentMode_When_Validate_Then_FailsOnDeploymentMode()
    {
        var command = ValidCommand() with { DeploymentMode = "" };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateWebAppCommand.DeploymentMode));
    }

    [Fact]
    public void Given_InvalidDeploymentMode_When_Validate_Then_FailsOnDeploymentMode()
    {
        var command = ValidCommand() with { DeploymentMode = "Docker" };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateWebAppCommand.DeploymentMode));
    }

    [Fact]
    public void Given_ContainerRegistryIdEmptyWhenProvided_When_Validate_Then_FailsOnContainerRegistryId()
    {
        var command = ValidCommand() with { ContainerRegistryId = Guid.Empty };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ContainerRegistryId.Value");
    }
}
