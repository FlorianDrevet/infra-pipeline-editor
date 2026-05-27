using System.Text.Json;
using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Projects.Commands.RemoveProjectResourceAbbreviation;
using InfraFlowSculptor.Application.Projects.Commands.RemoveProjectResourceNamingTemplate;
using InfraFlowSculptor.Application.Projects.Commands.SetProjectDefaultNamingTemplate;
using InfraFlowSculptor.Application.Projects.Commands.SetProjectResourceAbbreviation;
using InfraFlowSculptor.Application.Projects.Commands.SetProjectResourceNamingTemplate;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Mcp.Tools;
using MediatR;
using NSubstitute;

namespace InfraFlowSculptor.Mcp.Tests.Tools;

/// <summary>
/// Unit tests for <see cref="NamingTools"/>.
/// </summary>
public sealed class NamingToolsTests
{
    private readonly ISender _mediator = Substitute.For<ISender>();

    // ── SetProjectNamingTemplate ───────────────────────────────────────

    [Fact]
    public async Task SetProjectNamingTemplate_InvalidProjectId_ReturnsError()
    {
        // Act
        var json = await NamingTools.SetProjectNamingTemplate(
            _mediator, "not-a-guid", "{projectName}-{resourceAbbr}");

        // Assert
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("error").GetString().Should().Be("invalid_project_id");
    }

    [Fact]
    public async Task SetProjectNamingTemplate_ValidInput_ReturnsSuccess()
    {
        // Arrange
        var projectGuid = Guid.NewGuid();

        _mediator
            .Send(Arg.Any<SetProjectDefaultNamingTemplateCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ErrorOr<Success>>(Result.Success));

        // Act
        var json = await NamingTools.SetProjectNamingTemplate(
            _mediator, projectGuid.ToString(), "{projectName}-{resourceAbbr}-{envSuffix}");

        // Assert
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("status").GetString().Should().Be("success");
        doc.RootElement.GetProperty("message").GetString().Should().Contain("naming template updated");
    }

    [Fact]
    public async Task SetProjectNamingTemplate_MediatRError_ReturnsCommandFailed()
    {
        // Arrange
        var projectGuid = Guid.NewGuid();

        _mediator
            .Send(Arg.Any<SetProjectDefaultNamingTemplateCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ErrorOr<Success>>(
                Error.NotFound("NOT_FOUND", "Project not found.")));

        // Act
        var json = await NamingTools.SetProjectNamingTemplate(
            _mediator, projectGuid.ToString(), "{projectName}");

        // Assert
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("error").GetString().Should().Be("command_failed");
        doc.RootElement.GetProperty("message").GetString().Should().Contain("Project not found.");
    }

    // ── SetProjectResourceNamingTemplate ──────────────────────────────

    [Fact]
    public async Task SetProjectResourceNamingTemplate_InvalidProjectId_ReturnsError()
    {
        // Act
        var json = await NamingTools.SetProjectResourceNamingTemplate(
            _mediator, "bad", "KeyVault", "{projectName}-kv-{envSuffix}");

        // Assert
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("error").GetString().Should().Be("invalid_project_id");
    }

    [Fact]
    public async Task SetProjectResourceNamingTemplate_ValidInput_ReturnsSuccess()
    {
        // Arrange
        var projectGuid = Guid.NewGuid();
        var templateResult = new ProjectResourceNamingTemplateResult(
            new ProjectResourceNamingTemplateId(Guid.NewGuid()),
            "KeyVault",
            "{projectName}-kv-{envSuffix}");

        _mediator
            .Send(Arg.Any<SetProjectResourceNamingTemplateCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ErrorOr<ProjectResourceNamingTemplateResult>>(templateResult));

        // Act
        var json = await NamingTools.SetProjectResourceNamingTemplate(
            _mediator, projectGuid.ToString(), "KeyVault", "{projectName}-kv-{envSuffix}");

        // Assert
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("status").GetString().Should().Be("success");
        doc.RootElement.GetProperty("resourceType").GetString().Should().Be("KeyVault");
        doc.RootElement.GetProperty("template").GetString().Should().Be("{projectName}-kv-{envSuffix}");
    }

    [Fact]
    public async Task SetProjectResourceNamingTemplate_MediatRError_ReturnsCommandFailed()
    {
        // Arrange
        var projectGuid = Guid.NewGuid();

        _mediator
            .Send(Arg.Any<SetProjectResourceNamingTemplateCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ErrorOr<ProjectResourceNamingTemplateResult>>(
                Error.Validation("INVALID", "Invalid resource type.")));

        // Act
        var json = await NamingTools.SetProjectResourceNamingTemplate(
            _mediator, projectGuid.ToString(), "BadType", "tpl");

        // Assert
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("error").GetString().Should().Be("command_failed");
    }

    // ── RemoveProjectResourceNamingTemplate ───────────────────────────

    [Fact]
    public async Task RemoveProjectResourceNamingTemplate_InvalidProjectId_ReturnsError()
    {
        // Act
        var json = await NamingTools.RemoveProjectResourceNamingTemplate(
            _mediator, "bad", "KeyVault");

        // Assert
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("error").GetString().Should().Be("invalid_project_id");
    }

    [Fact]
    public async Task RemoveProjectResourceNamingTemplate_ValidInput_ReturnsSuccess()
    {
        // Arrange
        var projectGuid = Guid.NewGuid();

        _mediator
            .Send(Arg.Any<RemoveProjectResourceNamingTemplateCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ErrorOr<Deleted>>(Result.Deleted));

        // Act
        var json = await NamingTools.RemoveProjectResourceNamingTemplate(
            _mediator, projectGuid.ToString(), "KeyVault");

        // Assert
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("status").GetString().Should().Be("success");
        doc.RootElement.GetProperty("message").GetString().Should().Contain("KeyVault");
    }

    // ── SetProjectResourceAbbreviation ─────────────────────────────────

    [Fact]
    public async Task SetProjectResourceAbbreviation_InvalidProjectId_ReturnsError()
    {
        // Act
        var json = await NamingTools.SetProjectResourceAbbreviation(
            _mediator, "bad", "KeyVault", "kv");

        // Assert
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("error").GetString().Should().Be("invalid_project_id");
    }

    [Fact]
    public async Task SetProjectResourceAbbreviation_ValidInput_ReturnsSuccess()
    {
        // Arrange
        var projectGuid = Guid.NewGuid();
        var abbrResult = new ProjectResourceAbbreviationResult(
            new ProjectResourceAbbreviationId(Guid.NewGuid()),
            "KeyVault",
            "kv");

        _mediator
            .Send(Arg.Any<SetProjectResourceAbbreviationCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ErrorOr<ProjectResourceAbbreviationResult>>(abbrResult));

        // Act
        var json = await NamingTools.SetProjectResourceAbbreviation(
            _mediator, projectGuid.ToString(), "KeyVault", "kv");

        // Assert
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("status").GetString().Should().Be("success");
        doc.RootElement.GetProperty("resourceType").GetString().Should().Be("KeyVault");
        doc.RootElement.GetProperty("abbreviation").GetString().Should().Be("kv");
    }

    [Fact]
    public async Task SetProjectResourceAbbreviation_MediatRError_ReturnsCommandFailed()
    {
        // Arrange
        var projectGuid = Guid.NewGuid();

        _mediator
            .Send(Arg.Any<SetProjectResourceAbbreviationCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ErrorOr<ProjectResourceAbbreviationResult>>(
                Error.Validation("TOO_LONG", "Abbreviation must be at most 10 characters.")));

        // Act
        var json = await NamingTools.SetProjectResourceAbbreviation(
            _mediator, projectGuid.ToString(), "KeyVault", "toolongabbreviation");

        // Assert
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("error").GetString().Should().Be("command_failed");
    }

    // ── RemoveProjectResourceAbbreviation ──────────────────────────────

    [Fact]
    public async Task RemoveProjectResourceAbbreviation_InvalidProjectId_ReturnsError()
    {
        // Act
        var json = await NamingTools.RemoveProjectResourceAbbreviation(
            _mediator, "bad", "KeyVault");

        // Assert
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("error").GetString().Should().Be("invalid_project_id");
    }

    [Fact]
    public async Task RemoveProjectResourceAbbreviation_ValidInput_ReturnsSuccess()
    {
        // Arrange
        var projectGuid = Guid.NewGuid();

        _mediator
            .Send(Arg.Any<RemoveProjectResourceAbbreviationCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ErrorOr<Deleted>>(Result.Deleted));

        // Act
        var json = await NamingTools.RemoveProjectResourceAbbreviation(
            _mediator, projectGuid.ToString(), "KeyVault");

        // Assert
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("status").GetString().Should().Be("success");
        doc.RootElement.GetProperty("message").GetString().Should().Contain("KeyVault");
    }

    [Fact]
    public async Task RemoveProjectResourceAbbreviation_MediatRError_ReturnsCommandFailed()
    {
        // Arrange
        var projectGuid = Guid.NewGuid();

        _mediator
            .Send(Arg.Any<RemoveProjectResourceAbbreviationCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ErrorOr<Deleted>>(
                Error.NotFound("NOT_FOUND", "Abbreviation override not found.")));

        // Act
        var json = await NamingTools.RemoveProjectResourceAbbreviation(
            _mediator, projectGuid.ToString(), "KeyVault");

        // Assert
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("error").GetString().Should().Be("command_failed");
    }
}
