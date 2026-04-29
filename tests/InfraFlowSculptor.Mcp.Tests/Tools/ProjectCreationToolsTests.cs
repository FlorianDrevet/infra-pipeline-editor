using System.Text.Json;
using ErrorOr;
using InfraFlowSculptor.Application.Projects.Commands.CreateProjectWithSetup;
using FluentAssertions;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Mcp.Drafts;
using InfraFlowSculptor.Mcp.Drafts.Models;
using InfraFlowSculptor.Mcp.Tools.Models;
using InfraFlowSculptor.Mcp.Tools;
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
}
