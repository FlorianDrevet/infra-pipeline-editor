using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.AppServicePlans.Commands.CreateAppServicePlan;
using InfraFlowSculptor.Application.AppServicePlans.Common;
using InfraFlowSculptor.Application.Imports.Common.Creation;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.CreateInfraConfig;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.Application.KeyVaults.Commands.CreateKeyVault;
using InfraFlowSculptor.Application.KeyVaults.Common;
using InfraFlowSculptor.Application.ResourceGroup.Commands.CreateResourceGroup;
using InfraFlowSculptor.Application.ResourceGroups.Common;
using InfraFlowSculptor.Application.WebApps.Commands.CreateWebApp;
using InfraFlowSculptor.Application.WebApps.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using InfraFlowSculptor.GenerationCore;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace InfraFlowSculptor.Application.Tests.Imports.Common.Creation;

/// <summary>
/// Tests for <see cref="ResourceCreationCoordinator"/> — the shared orchestration layer.
/// Validates infrastructure creation delegation, dependency name precedence, ambiguity detection,
/// and proper error logging.
/// </summary>
public sealed class ResourceCreationCoordinatorTests
{
    private static readonly ResourceGroupId TestRgId = new(Guid.NewGuid());

    #region CreateInfrastructureAsync

    [Fact]
    public async Task Given_ValidProject_When_CreateInfrastructureAsync_Then_ReturnsConfigAndResourceGroup()
    {
        // Arrange
        var mediator = Substitute.For<ISender>();
        var configId = new InfrastructureConfigId(Guid.NewGuid());
        var rgId = new ResourceGroupId(Guid.NewGuid());

        mediator.Send(Arg.Any<CreateInfrastructureConfigCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ErrorOr<GetInfrastructureConfigResult>>(
                new GetInfrastructureConfigResult(configId, new Name("test-config"), new(Guid.NewGuid()), null, false, [], [], [], 0, 0, 0, null, null)));
        mediator.Send(Arg.Any<CreateResourceGroupCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ErrorOr<ResourceGroupResult>>(
                new ResourceGroupResult(rgId, configId, new Location(Location.LocationEnum.WestEurope), new Name("test-rg"), [])));

        // Act
        var result = await ResourceCreationCoordinator.CreateInfrastructureAsync(
            mediator, Guid.NewGuid(), "my-project", nameof(Location.LocationEnum.WestEurope), CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.ConfigId.Should().Be(configId);
        result.Value.ResourceGroupId.Should().Be(rgId);
    }

    [Fact]
    public async Task Given_InfraConfigFails_When_CreateInfrastructureAsync_Then_ReturnsError()
    {
        // Arrange
        var mediator = Substitute.For<ISender>();
        mediator.Send(Arg.Any<CreateInfrastructureConfigCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ErrorOr<GetInfrastructureConfigResult>>(Error.Failure("config.fail", "boom")));

        // Act
        var result = await ResourceCreationCoordinator.CreateInfrastructureAsync(
            mediator, Guid.NewGuid(), "my-project", nameof(Location.LocationEnum.WestEurope), CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("config.fail");
    }

    #endregion

    #region CreateResourcesAsync — Dependency Name Precedence

    [Fact]
    public async Task Given_ExplicitDependencyName_When_MultipleOfSameType_Then_ResolvesNamedDependency()
    {
        // Arrange: 2 AppServicePlans + 1 WebApp that explicitly depends on plan-B
        var mediator = Substitute.For<ISender>();
        var planAId = Guid.NewGuid();
        var planBId = Guid.NewGuid();
        var webAppId = Guid.NewGuid();

        var defaultLocation = new Location(Location.LocationEnum.WestEurope);

        // Plan A creation → returns planAId
        mediator.Send(Arg.Is<CreateAppServicePlanCommand>(c => c.Name.Value == "plan-A"), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ErrorOr<AppServicePlanResult>>(
                new AppServicePlanResult(new AzureResourceId(planAId), TestRgId, new Name("plan-A"), defaultLocation, "Linux", [])));
        // Plan B creation → returns planBId
        mediator.Send(Arg.Is<CreateAppServicePlanCommand>(c => c.Name.Value == "plan-B"), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ErrorOr<AppServicePlanResult>>(
                new AppServicePlanResult(new AzureResourceId(planBId), TestRgId, new Name("plan-B"), defaultLocation, "Linux", [])));
        // WebApp creation → capture the command to verify AppServicePlanId
        CreateWebAppCommand? capturedWebApp = null;
        mediator.Send(Arg.Any<CreateWebAppCommand>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                capturedWebApp = callInfo.Arg<CreateWebAppCommand>();
                return Task.FromResult<ErrorOr<WebAppResult>>(
                    new WebAppResult(new AzureResourceId(webAppId), TestRgId, new Name("my-web"), defaultLocation,
                        callInfo.Arg<CreateWebAppCommand>().AppServicePlanId,
                        ".NET", "10.0", true, true, "code", null, null, null, null, null, null, null, []));
            });

        var inputs = new List<ResourceCreationInput>
        {
            new() { ResourceType = AzureResourceTypes.AppServicePlan, Name = "plan-A" },
            new() { ResourceType = AzureResourceTypes.AppServicePlan, Name = "plan-B" },
            new() { ResourceType = AzureResourceTypes.WebApp, Name = "my-web", DependencyResourceNames = ["plan-B"] },
        };

        // Act
        var (created, skipped) = await ResourceCreationCoordinator.CreateResourcesAsync(
            mediator, TestRgId, inputs, cancellationToken: CancellationToken.None);

        // Assert
        skipped.Should().BeEmpty();
        created.Should().HaveCount(3);
        capturedWebApp.Should().NotBeNull();
        capturedWebApp!.AppServicePlanId.Should().Be(planBId);
    }

    [Fact]
    public async Task Given_NoDependencyNames_When_MultipleOfSameType_Then_SkipsWithAmbiguity()
    {
        // Arrange: 2 AppServicePlans + 1 WebApp without explicit dependency
        var mediator = Substitute.For<ISender>();
        var defaultLocation = new Location(Location.LocationEnum.WestEurope);

        mediator.Send(Arg.Any<CreateAppServicePlanCommand>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var id = Guid.NewGuid();
                return Task.FromResult<ErrorOr<AppServicePlanResult>>(
                    new AppServicePlanResult(new AzureResourceId(id), TestRgId, callInfo.Arg<CreateAppServicePlanCommand>().Name, defaultLocation, "Linux", []));
            });

        var inputs = new List<ResourceCreationInput>
        {
            new() { ResourceType = AzureResourceTypes.AppServicePlan, Name = "plan-A" },
            new() { ResourceType = AzureResourceTypes.AppServicePlan, Name = "plan-B" },
            new() { ResourceType = AzureResourceTypes.WebApp, Name = "my-web" },
        };

        // Act
        var (created, skipped) = await ResourceCreationCoordinator.CreateResourcesAsync(
            mediator, TestRgId, inputs, cancellationToken: CancellationToken.None);

        // Assert
        skipped.Should().ContainSingle();
        skipped[0].ResourceType.Should().Be(AzureResourceTypes.WebApp);
        skipped[0].Reason.Should().Contain("Ambiguous dependency");
        created.Should().HaveCount(2);
    }

    #endregion

    #region CreateResourcesAsync — Error Logging

    [Fact]
    public async Task Given_UnexpectedException_When_CreateResourcesAsync_Then_LogsError()
    {
        // Arrange
        var mediator = Substitute.For<ISender>();
        var logger = Substitute.For<ILogger>();

        mediator.Send(Arg.Any<CreateKeyVaultCommand>(), Arg.Any<CancellationToken>())
            .Returns<Task<ErrorOr<KeyVaultResult>>>(_ => throw new InvalidOperationException("boom"));

        var inputs = new List<ResourceCreationInput>
        {
            new() { ResourceType = AzureResourceTypes.KeyVault, Name = "kv-test" },
        };

        // Act
        var (created, skipped) = await ResourceCreationCoordinator.CreateResourcesAsync(
            mediator, TestRgId, inputs, logger, CancellationToken.None);

        // Assert
        created.Should().BeEmpty();
        skipped.Should().ContainSingle();
        skipped[0].Reason.Should().Contain("InvalidOperationException");
        logger.ReceivedWithAnyArgs().Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    #endregion

    #region CreateResourcesAsync — Independent Resources

    [Fact]
    public async Task Given_SingleIndependentResource_When_CreateResourcesAsync_Then_ReturnsCreated()
    {
        // Arrange
        var mediator = Substitute.For<ISender>();
        var kvId = Guid.NewGuid();
        var defaultLocation = new Location(Location.LocationEnum.WestEurope);

        mediator.Send(Arg.Any<CreateKeyVaultCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ErrorOr<KeyVaultResult>>(
                new KeyVaultResult(new AzureResourceId(kvId), TestRgId, new Name("kv-1"), defaultLocation,
                    true, false, false, false, true, true, [])));

        var inputs = new List<ResourceCreationInput>
        {
            new() { ResourceType = AzureResourceTypes.KeyVault, Name = "kv-1" },
        };

        // Act
        var (created, skipped) = await ResourceCreationCoordinator.CreateResourcesAsync(
            mediator, TestRgId, inputs, cancellationToken: CancellationToken.None);

        // Assert
        skipped.Should().BeEmpty();
        created.Should().ContainSingle();
        created[0].ResourceType.Should().Be(AzureResourceTypes.KeyVault);
        created[0].ResourceId.Should().Be(kvId.ToString());
        created[0].Name.Should().Be("kv-1");
    }

    #endregion
}
