using System.Text.Json;
using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Projects.Commands.GenerateProjectBicep;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Mcp.Tools;
using MediatR;
using NSubstitute;

namespace InfraFlowSculptor.Mcp.Tests.Tools;

/// <summary>
/// Unit tests for <see cref="BicepGenerationTools"/>.
/// </summary>
public sealed class BicepGenerationToolsTests
{
    private readonly ISender _mediator = Substitute.For<ISender>();

    [Fact]
    public async Task GenerateProjectBicep_InvalidGuid_ReturnsError()
    {
        // Arrange
        const string invalidId = "not-a-guid";

        // Act
        var json = await BicepGenerationTools.GenerateProjectBicep(_mediator, invalidId);

        // Assert
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("error").GetString().Should().Be("invalid_project_id");
        doc.RootElement.GetProperty("message").GetString().Should().Contain("not-a-guid");
    }

    [Fact]
    public async Task GenerateProjectBicep_ValidGuid_SendsCommandViaMediatR()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var projectId = new ProjectId(guid);
        var commonFiles = new Dictionary<string, Uri>
        {
            ["types.bicep"] = new("https://blob.example.com/types.bicep"),
            ["functions.bicep"] = new("https://blob.example.com/functions.bicep"),
            ["modules/kv.bicep"] = new("https://blob.example.com/modules/kv.bicep"),
        };
        var configFiles = new Dictionary<string, IReadOnlyDictionary<string, Uri>>
        {
            ["main-config"] = new Dictionary<string, Uri>
            {
                ["main.bicep"] = new("https://blob.example.com/main.bicep"),
                ["parameters/dev.bicepparam"] = new("https://blob.example.com/parameters/dev.bicepparam"),
            },
        };
        var result = new GenerateProjectBicepResult(commonFiles, configFiles);

        _mediator
            .Send(Arg.Any<IRequest<ErrorOr<GenerateProjectBicepResult>>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ErrorOr<GenerateProjectBicepResult>>(result));

        // Act
        var json = await BicepGenerationTools.GenerateProjectBicep(_mediator, guid.ToString());

        // Assert
        await _mediator.Received(1).Send(
            Arg.Any<IRequest<ErrorOr<GenerateProjectBicepResult>>>(),
            Arg.Any<CancellationToken>());

        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("status").GetString().Should().Be("generated");
        doc.RootElement.GetProperty("projectId").GetString().Should().Be(guid.ToString());
    }

    [Fact]
    public async Task GenerateProjectBicep_MediatRError_ReturnsErrorJson()
    {
        // Arrange
        var guid = Guid.NewGuid();

        _mediator
            .Send(Arg.Any<IRequest<ErrorOr<GenerateProjectBicepResult>>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ErrorOr<GenerateProjectBicepResult>>(Error.Failure("GEN_FAIL", "Bicep generation failed.")));

        // Act
        var json = await BicepGenerationTools.GenerateProjectBicep(_mediator, guid.ToString());

        // Assert
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("error").GetString().Should().Be("generation_failed");
        doc.RootElement.GetProperty("message").GetString().Should().Contain("Bicep generation failed.");
    }

    [Fact]
    public async Task GenerateProjectBicep_Success_ReturnsTotalFileCount()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var commonFiles = new Dictionary<string, Uri>
        {
            ["types.bicep"] = new("https://blob.example.com/types.bicep"),
            ["functions.bicep"] = new("https://blob.example.com/functions.bicep"),
        };
        var configFiles = new Dictionary<string, IReadOnlyDictionary<string, Uri>>
        {
            ["config-a"] = new Dictionary<string, Uri>
            {
                ["main.bicep"] = new("https://blob.example.com/main.bicep"),
            },
            ["config-b"] = new Dictionary<string, Uri>
            {
                ["main.bicep"] = new("https://blob.example.com/main-b.bicep"),
                ["params.bicepparam"] = new("https://blob.example.com/params-b.bicepparam"),
            },
        };
        var result = new GenerateProjectBicepResult(commonFiles, configFiles);

        _mediator
            .Send(Arg.Any<IRequest<ErrorOr<GenerateProjectBicepResult>>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ErrorOr<GenerateProjectBicepResult>>(result));

        // Act
        var json = await BicepGenerationTools.GenerateProjectBicep(_mediator, guid.ToString());

        // Assert
        var doc = JsonDocument.Parse(json);
        // 2 common + 1 config-a + 2 config-b = 5
        doc.RootElement.GetProperty("totalFileCount").GetInt32().Should().Be(5);
    }
}
