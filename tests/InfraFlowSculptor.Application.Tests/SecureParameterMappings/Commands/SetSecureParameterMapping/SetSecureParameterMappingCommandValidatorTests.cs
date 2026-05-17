using FluentAssertions;
using InfraFlowSculptor.Application.SecureParameterMappings.Commands.SetSecureParameterMapping;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.SecureParameterMappings.Commands.SetSecureParameterMapping;

public sealed class SetSecureParameterMappingCommandValidatorTests
{
    private readonly SetSecureParameterMappingCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommandWithGroup_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new SetSecureParameterMappingCommand(
            AzureResourceId.CreateUnique(),
            "sqlAdminPassword",
            ProjectPipelineVariableGroupId.CreateUnique(),
            "sql_admin_password");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_ValidClear_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new SetSecureParameterMappingCommand(
            AzureResourceId.CreateUnique(),
            "sqlAdminPassword",
            VariableGroupId: null,
            PipelineVariableName: null);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_EmptySecureParameterName_When_Validate_Then_Fails()
    {
        // Arrange
        var command = new SetSecureParameterMappingCommand(
            AzureResourceId.CreateUnique(),
            SecureParameterName: "",
            VariableGroupId: null,
            PipelineVariableName: null);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SetSecureParameterMappingCommand.SecureParameterName));
    }

    [Fact]
    public void Given_MissingVariableNameWithGroupId_When_Validate_Then_Fails()
    {
        // Arrange
        var command = new SetSecureParameterMappingCommand(
            AzureResourceId.CreateUnique(),
            "sqlAdminPassword",
            ProjectPipelineVariableGroupId.CreateUnique(),
            PipelineVariableName: null);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SetSecureParameterMappingCommand.PipelineVariableName));
    }

    [Fact]
    public void Given_InvalidVariableNamePattern_When_Validate_Then_Fails()
    {
        // Arrange
        var command = new SetSecureParameterMappingCommand(
            AzureResourceId.CreateUnique(),
            "sqlAdminPassword",
            ProjectPipelineVariableGroupId.CreateUnique(),
            PipelineVariableName: "my-var");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SetSecureParameterMappingCommand.PipelineVariableName));
    }

    [Fact]
    public void Given_NonNullVariableNameWithNullGroupId_When_Validate_Then_Fails()
    {
        // Arrange
        var command = new SetSecureParameterMappingCommand(
            AzureResourceId.CreateUnique(),
            "sqlAdminPassword",
            VariableGroupId: null,
            PipelineVariableName: "some_var");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SetSecureParameterMappingCommand.PipelineVariableName));
    }
}
