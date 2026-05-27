using FluentAssertions;
using InfraFlowSculptor.Application.Projects.Commands.AddProjectMember;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.Projects.Commands.AddProjectMember;

public sealed class AddProjectMemberCommandValidatorTests
{
    private const string ValidRole = "Contributor";
    private const string ProjectIdProperty = nameof(AddProjectMemberCommand.ProjectId);
    private const string UserIdProperty = nameof(AddProjectMemberCommand.UserId);
    private const string RoleProperty = nameof(AddProjectMemberCommand.Role);

    private readonly AddProjectMemberCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new AddProjectMemberCommand(ProjectId.CreateUnique(), Guid.NewGuid(), ValidRole);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_NullProjectId_When_Validate_Then_FailsOnProjectId()
    {
        // Arrange
        var command = new AddProjectMemberCommand(null!, Guid.NewGuid(), ValidRole);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == ProjectIdProperty);
    }

    [Fact]
    public void Given_EmptyUserId_When_Validate_Then_FailsOnUserId()
    {
        // Arrange
        var command = new AddProjectMemberCommand(ProjectId.CreateUnique(), Guid.Empty, ValidRole);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == UserIdProperty);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Given_EmptyRole_When_Validate_Then_FailsOnRole(string? role)
    {
        // Arrange
        var command = new AddProjectMemberCommand(ProjectId.CreateUnique(), Guid.NewGuid(), role!);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == RoleProperty);
    }
}
