using FluentAssertions;
using InfraFlowSculptor.Application.Projects.Commands.DownloadProjectBicep;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.Projects.Commands.DownloadProjectBicep;

public sealed class DownloadProjectBicepCommandValidatorTests
{
    private const string ProjectIdProperty = "ProjectId.Value";

    private readonly DownloadProjectBicepCommandValidator _sut = new();

    [Fact]
    public void Given_ValidProjectId_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new DownloadProjectBicepCommand(ProjectId.CreateUnique());

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_EmptyProjectId_When_Validate_Then_FailsOnProjectId()
    {
        // Arrange
        var command = new DownloadProjectBicepCommand(new ProjectId(Guid.Empty));

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == ProjectIdProperty);
    }
}
