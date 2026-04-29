using System.Text.Json;
using ErrorOr;
using InfraFlowSculptor.Application.ContainerAppEnvironments.Commands.CreateContainerAppEnvironment;
using InfraFlowSculptor.Application.ContainerAppEnvironments.Common;
using InfraFlowSculptor.Application.ContainerApps.Commands.CreateContainerApp;
using InfraFlowSculptor.Application.ContainerApps.Common;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.CreateInfraConfig;
using InfraFlowSculptor.Application.Projects.Commands.CreateProjectWithSetup;
using FluentAssertions;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Application.ResourceGroup.Commands.CreateResourceGroup;
using InfraFlowSculptor.Application.ResourceGroups.Common;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using InfraFlowSculptor.Mcp.Drafts;
using InfraFlowSculptor.Mcp.Drafts.Models;
using InfraFlowSculptor.Mcp.Tools.Models;
using InfraFlowSculptor.Mcp.Tools;
using InfraFlowSculptor.GenerationCore;
using MediatR;
using NSubstitute;

namespace InfraFlowSculptor.Mcp.Tests.Tools;

public sealed class ProjectCreationToolsTests
{
    private readonly IProjectDraftService _draftService = Substitute.For<IProjectDraftService>();
    private readonly ISender _mediator = Substitute.For<ISender>();

    [Fact]
    public async Task CreateProjectFromDraft_DraftNotFound_ReturnsError()
    {
        // Arrange
        _draftService.GetDraft("draft_unknown").Returns((ProjectCreationDraft?)null);

        // Act
        var json = await ProjectCreationTools.CreateProjectFromDraft(_draftService, _mediator, "draft_unknown");

        // Assert
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("error").GetString().Should().Be("draft_not_found");
    }

    [Fact]
    public async Task CreateProjectFromDraft_DraftNotReady_ReturnsError()
    {
        // Arrange
        var draft = new ProjectCreationDraft
        {
            DraftId = "draft_abc12345",
            Status = DraftStatus.RequiresClarification,
        };
        _draftService.GetDraft("draft_abc12345").Returns(draft);

        // Act
        var json = await ProjectCreationTools.CreateProjectFromDraft(_draftService, _mediator, "draft_abc12345");

        // Assert
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("error").GetString().Should().Be("draft_not_ready");
    }

    [Fact]
    public async Task CreateProjectFromDraft_ReadyDraft_SendsCommandViaMediatR()
    {
        // Arrange
        var draft = new ProjectCreationDraft
        {
            DraftId = "draft_abc12345",
            Status = DraftStatus.ReadyToCreate,
            Intent = new DraftProjectIntent
            {
                ProjectName = "RetailApi",
                LayoutPreset = LayoutPresetEnum.AllInOne,
                Environments =
                [
                    new DraftEnvironmentIntent { Name = "Development", ShortName = "dev" },
                ],
                Repositories =
                [
                    new DraftRepositoryIntent { Alias = "main", ContentKinds = ["Infrastructure", "ApplicationCode"] },
                ],
            },
        };
        _draftService.GetDraft("draft_abc12345").Returns(draft);

        var projectId = new ProjectId(Guid.NewGuid());
        var projectName = new Name("RetailApi");
        var projectResult = new ProjectResult(
            projectId,
            projectName,
            Description: null,
            Members: [],
            EnvironmentDefinitions: [],
            DefaultNamingTemplate: null,
            ResourceNamingTemplates: [],
            ResourceAbbreviations: [],
            Tags: [],
            LayoutPreset: "AllInOne");

        _mediator.Send(Arg.Any<IRequest<ErrorOr<ProjectResult>>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ErrorOr<ProjectResult>>(projectResult));

        // Act
        var json = await ProjectCreationTools.CreateProjectFromDraft(_draftService, _mediator, "draft_abc12345");

        // Assert
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("status").GetString().Should().Be("created");
        doc.RootElement.GetProperty("projectId").GetString().Should().Be(projectId.Value.ToString());
        doc.RootElement.GetProperty("projectName").GetString().Should().Be("RetailApi");

        await _mediator.Received(1).Send(
            Arg.Is<CreateProjectWithSetupCommand>(command =>
                command.Repositories.Count == 1
                && command.Repositories[0].ContentKinds.Count == 2
                && command.Repositories[0].ContentKinds[0] == "Infrastructure"
                && command.Repositories[0].ContentKinds[1] == "ApplicationCode"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateProjectFromDraft_MediatRError_ReturnsError()
    {
        // Arrange
        var draft = new ProjectCreationDraft
        {
            DraftId = "draft_abc12345",
            Status = DraftStatus.ReadyToCreate,
            Intent = new DraftProjectIntent
            {
                ProjectName = "RetailApi",
                LayoutPreset = LayoutPresetEnum.AllInOne,
                Environments = [new DraftEnvironmentIntent()],
                Repositories = [new DraftRepositoryIntent { Alias = "main", ContentKinds = ["Infrastructure", "ApplicationCode"] }],
            },
        };
        _draftService.GetDraft("draft_abc12345").Returns(draft);

        _mediator.Send(Arg.Any<IRequest<ErrorOr<ProjectResult>>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ErrorOr<ProjectResult>>(Error.Validation("Name.Duplicate", "A project with this name already exists.")));

        // Act
        var json = await ProjectCreationTools.CreateProjectFromDraft(_draftService, _mediator, "draft_abc12345");

        // Assert
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("error").GetString().Should().Be("creation_failed");
        doc.RootElement.GetProperty("message").GetString().Should().Contain("already exists");
    }

    [Fact]
    public async Task CreateProjectFromDraft_ReadyDraftWithoutSubscription_ReturnsDeferredConfigurationWarning()
    {
        // Arrange
        var draft = new ProjectCreationDraft
        {
            DraftId = "draft_abc12345",
            Status = DraftStatus.ReadyToCreate,
            Intent = new DraftProjectIntent
            {
                ProjectName = "RetailApi",
                LayoutPreset = LayoutPresetEnum.AllInOne,
                Environments =
                [
                    new DraftEnvironmentIntent { Name = "Development", ShortName = "dev", SubscriptionId = Guid.Empty },
                ],
                Repositories =
                [
                    new DraftRepositoryIntent { Alias = "main", ContentKinds = ["Infrastructure", "ApplicationCode"] },
                ],
            },
        };
        _draftService.GetDraft("draft_abc12345").Returns(draft);

        var projectId = new ProjectId(Guid.NewGuid());
        var projectName = new Name("RetailApi");
        var projectResult = new ProjectResult(
            projectId,
            projectName,
            Description: null,
            Members: [],
            EnvironmentDefinitions: [],
            DefaultNamingTemplate: null,
            ResourceNamingTemplates: [],
            ResourceAbbreviations: [],
            Tags: [],
            LayoutPreset: "AllInOne");

        _mediator.Send(Arg.Any<IRequest<ErrorOr<ProjectResult>>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ErrorOr<ProjectResult>>(projectResult));

        // Act
        var json = await ProjectCreationTools.CreateProjectFromDraft(_draftService, _mediator, "draft_abc12345");

        // Assert
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("status").GetString().Should().Be("created");
        doc.RootElement.GetProperty("warnings").EnumerateArray()
            .Select(item => item.GetString())
            .Should().Contain(message => message != null
                && message.Contains("subscription", StringComparison.OrdinalIgnoreCase)
                && message.Contains("later", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Given_ReadyDraftWithContainerAppOnly_When_CreateProjectFromDraft_Then_CreatesManagedEnvironmentDependency()
    {
        // Arrange
        var draft = new ProjectCreationDraft
        {
            DraftId = "draft_containerapp",
            Status = DraftStatus.ReadyToCreate,
            Intent = new DraftProjectIntent
            {
                ProjectName = "RetailApi",
                LayoutPreset = LayoutPresetEnum.AllInOne,
                Environments =
                [
                    new DraftEnvironmentIntent { Name = "Production", ShortName = "prod", Location = Location.ToAzureRegionKey(Location.LocationEnum.FranceCentral) },
                ],
                Repositories =
                [
                    new DraftRepositoryIntent { Alias = "main", ContentKinds = ["Infrastructure", "ApplicationCode"] },
                ],
                Resources =
                [
                    new DraftResourceIntent { ResourceType = AzureResourceTypes.ContainerApp, Name = "retail-api" },
                ],
            },
        };
        _draftService.GetDraft(draft.DraftId).Returns(draft);

        var projectId = new ProjectId(Guid.NewGuid());
        var projectResult = new ProjectResult(
            projectId,
            new Name("RetailApi"),
            Description: null,
            Members: [],
            EnvironmentDefinitions: [],
            DefaultNamingTemplate: null,
            ResourceNamingTemplates: [],
            ResourceAbbreviations: [],
            Tags: [],
            LayoutPreset: "AllInOne");

        var infrastructureConfigId = new InfrastructureConfigId(Guid.NewGuid());
        var resourceGroupId = new ResourceGroupId(Guid.NewGuid());
        var containerAppEnvironmentId = new AzureResourceId(Guid.NewGuid());
        var containerAppId = new AzureResourceId(Guid.NewGuid());
        var franceCentral = new Location(Location.LocationEnum.FranceCentral);

        _mediator.Send(Arg.Any<CreateProjectWithSetupCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ErrorOr<ProjectResult>>(projectResult));

        _mediator.Send(Arg.Any<CreateInfrastructureConfigCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ErrorOr<GetInfrastructureConfigResult>>(
                new GetInfrastructureConfigResult(
                    infrastructureConfigId,
                    new Name("RetailApi-config"),
                    projectId,
                    null,
                    false,
                    [],
                    [],
                    [],
                    0,
                    0,
                    0,
                    null,
                    null)));

        _mediator.Send(Arg.Any<CreateResourceGroupCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ErrorOr<ResourceGroupResult>>(
                new ResourceGroupResult(
                    resourceGroupId,
                    infrastructureConfigId,
                    franceCentral,
                    new Name("RetailApi-rg"),
                    [])));

        _mediator.Send(Arg.Any<CreateContainerAppEnvironmentCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ErrorOr<ContainerAppEnvironmentResult>>(
                new ContainerAppEnvironmentResult(
                    containerAppEnvironmentId,
                    resourceGroupId,
                    new Name("RetailApi-containerappenvironment"),
                    franceCentral,
                    null,
                    [])));

        _mediator.Send(Arg.Any<CreateContainerAppCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ErrorOr<ContainerAppResult>>(
                new ContainerAppResult(
                    containerAppId,
                    resourceGroupId,
                    new Name("retail-api"),
                    franceCentral,
                    containerAppEnvironmentId.Value,
                    null,
                    null,
                    null,
                    null,
                    null,
                    [])));

        // Act
        var json = await ProjectCreationTools.CreateProjectFromDraft(_draftService, _mediator, draft.DraftId);

        // Assert
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("status").GetString().Should().Be("created");
        doc.RootElement.GetProperty("createdResources").EnumerateArray()
            .Select(resource => resource.GetProperty("resourceType").GetString())
            .Should().BeEquivalentTo([AzureResourceTypes.ContainerAppEnvironment, AzureResourceTypes.ContainerApp]);
        doc.RootElement.GetProperty("skippedResources").EnumerateArray().Should().BeEmpty();

        await _mediator.Received(1).Send(
            Arg.Any<CreateContainerAppEnvironmentCommand>(),
            Arg.Any<CancellationToken>());
        await _mediator.Received(1).Send(
            Arg.Is<CreateContainerAppCommand>(command =>
                command.Name.Value == "retail-api"
                && command.ContainerAppEnvironmentId == containerAppEnvironmentId.Value),
            Arg.Any<CancellationToken>());
    }
}
