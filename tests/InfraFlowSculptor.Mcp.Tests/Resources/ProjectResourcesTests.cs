using System.Text.Json;
using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Application.Projects.Queries.GetProject;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Mcp.Resources;
using MediatR;
using NSubstitute;

namespace InfraFlowSculptor.Mcp.Tests.Resources;

/// <summary>
/// Unit tests for <see cref="ProjectResources"/>.
/// </summary>
public sealed class ProjectResourcesTests
{
    private readonly ISender _mediator = Substitute.For<ISender>();

    [Fact]
    public async Task GetProjectSummary_InvalidGuid_ReturnsError()
    {
        // Arrange
        const string invalidId = "not-a-guid";

        // Act
        var json = await ProjectResources.GetProjectSummary(_mediator, invalidId);

        // Assert
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("error").GetString().Should().Be("invalid_project_id");
        doc.RootElement.GetProperty("message").GetString().Should().Contain("not-a-guid");
    }

    [Fact]
    public async Task GetProjectSummary_ProjectNotFound_ReturnsError()
    {
        // Arrange
        var guid = Guid.NewGuid();

        _mediator
            .Send(Arg.Any<IRequest<ErrorOr<ProjectResult>>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ErrorOr<ProjectResult>>(Error.NotFound("NOT_FOUND", "Project not found.")));

        // Act
        var json = await ProjectResources.GetProjectSummary(_mediator, guid.ToString());

        // Assert
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("error").GetString().Should().Be("project_not_found");
        doc.RootElement.GetProperty("message").GetString().Should().Contain("Project not found.");
    }

    [Fact]
    public async Task GetProjectSummary_ValidProject_ReturnsJsonSummary()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var projectId = new ProjectId(guid);
        var projectResult = new ProjectResult(
            Id: projectId,
            Name: new Name("TestProject"),
            Description: "A test project",
            Members: [],
            EnvironmentDefinitions:
            [
                new ProjectEnvironmentDefinitionResult(
                    Id: new ProjectEnvironmentDefinitionId(Guid.NewGuid()),
                    Name: new Name("Development"),
                    ShortName: "dev",
                    Prefix: "",
                    Suffix: "",
                    Location: Location.DefaultAzureRegionKey,
                    SubscriptionId: Guid.NewGuid(),
                    Order: 1,
                    RequiresApproval: false,
                    AzureResourceManagerConnection: null,
                    Tags: []),
            ],
            DefaultNamingTemplate: null,
            ResourceNamingTemplates: [],
            ResourceAbbreviations: [],
            Tags: [],
            AgentPoolName: null,
            UsedResourceTypes: ["KeyVault"],
            Repositories: [],
            LayoutPreset: "AllInOne");

        _mediator
            .Send(Arg.Any<IRequest<ErrorOr<ProjectResult>>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ErrorOr<ProjectResult>>(projectResult));

        // Act
        var json = await ProjectResources.GetProjectSummary(_mediator, guid.ToString());

        // Assert
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("projectId").GetString().Should().Be(guid.ToString());
        doc.RootElement.GetProperty("name").GetString().Should().Be("TestProject");
        doc.RootElement.GetProperty("description").GetString().Should().Be("A test project");
        doc.RootElement.GetProperty("layoutPreset").GetString().Should().Be("AllInOne");
        doc.RootElement.GetProperty("environmentCount").GetInt32().Should().Be(1);
        doc.RootElement.GetProperty("repositoryCount").GetInt32().Should().Be(0);

        var environments = doc.RootElement.GetProperty("environments");
        environments.GetArrayLength().Should().Be(1);
        environments[0].GetProperty("name").GetString().Should().Be("Development");
        environments[0].GetProperty("shortName").GetString().Should().Be("dev");
        environments[0].GetProperty("location").GetString().Should().Be(Location.DefaultAzureRegionKey);

        var resourceTypes = doc.RootElement.GetProperty("usedResourceTypes");
        resourceTypes.GetArrayLength().Should().Be(1);
        resourceTypes[0].GetString().Should().Be("KeyVault");
    }
}
