using FluentAssertions;
using InfraFlowSculptor.Application.Projects.Commands.UpdateProjectEnvironment;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.Projects.Commands.UpdateProjectEnvironment;

public sealed class UpdateProjectEnvironmentCommandValidatorTests
{
    private const string NameProperty = nameof(UpdateProjectEnvironmentCommand.Name);
    private const string PrefixProperty = nameof(UpdateProjectEnvironmentCommand.Prefix);
    private const string SuffixProperty = nameof(UpdateProjectEnvironmentCommand.Suffix);
    private const string LocationProperty = nameof(UpdateProjectEnvironmentCommand.Location);
    private const string SubscriptionIdProperty = nameof(UpdateProjectEnvironmentCommand.SubscriptionId);
    private const string OrderProperty = nameof(UpdateProjectEnvironmentCommand.Order);

    private readonly UpdateProjectEnvironmentCommandValidator _sut = new();

    private static UpdateProjectEnvironmentCommand CreateValidCommand(
        string name = "Production",
        string prefix = "prd",
        string suffix = "",
        string location = "westeurope",
        int order = 0,
        IReadOnlyList<(string Name, string Value)>? tags = null)
    {
        return new UpdateProjectEnvironmentCommand(
            ProjectId.CreateUnique(),
            ProjectEnvironmentDefinitionId.CreateUnique(),
            name,
            "prd",
            prefix,
            suffix,
            location,
            Guid.NewGuid(),
            order,
            false,
            null,
            tags ?? []);
    }

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = CreateValidCommand();

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_EmptyName_When_Validate_Then_FailsOnName()
    {
        // Arrange
        var command = CreateValidCommand(name: "");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == NameProperty);
    }

    [Fact]
    public void Given_NameTooLong_When_Validate_Then_FailsOnName()
    {
        // Arrange
        var command = CreateValidCommand(name: new string('a', 101));

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == NameProperty);
    }

    [Fact]
    public void Given_NullPrefix_When_Validate_Then_FailsOnPrefix()
    {
        // Arrange
        var command = new UpdateProjectEnvironmentCommand(
            ProjectId.CreateUnique(),
            ProjectEnvironmentDefinitionId.CreateUnique(),
            "Production", "prd", null!, "", "westeurope",
            Guid.NewGuid(), 0, false, null, []);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == PrefixProperty);
    }

    [Fact]
    public void Given_PrefixTooLong_When_Validate_Then_FailsOnPrefix()
    {
        // Arrange
        var command = CreateValidCommand(prefix: new string('a', 51));

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == PrefixProperty);
    }

    [Fact]
    public void Given_NullSuffix_When_Validate_Then_FailsOnSuffix()
    {
        // Arrange
        var command = new UpdateProjectEnvironmentCommand(
            ProjectId.CreateUnique(),
            ProjectEnvironmentDefinitionId.CreateUnique(),
            "Production", "prd", "prd", null!, "westeurope",
            Guid.NewGuid(), 0, false, null, []);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == SuffixProperty);
    }

    [Fact]
    public void Given_SuffixTooLong_When_Validate_Then_FailsOnSuffix()
    {
        // Arrange
        var command = CreateValidCommand(suffix: new string('a', 51));

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == SuffixProperty);
    }

    [Fact]
    public void Given_EmptyLocation_When_Validate_Then_FailsOnLocation()
    {
        // Arrange
        var command = CreateValidCommand(location: "");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == LocationProperty);
    }

    [Fact]
    public void Given_EmptySubscriptionId_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new UpdateProjectEnvironmentCommand(
            ProjectId.CreateUnique(),
            ProjectEnvironmentDefinitionId.CreateUnique(),
            "Production", "prd", "prd", "", "westeurope",
            Guid.Empty, 0, false, null, []);

        // Act
        var result = _sut.Validate(command);

        // Assert — SubscriptionId is optional (Guid.Empty means "not configured yet")
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Given_NegativeOrder_When_Validate_Then_FailsOnOrder()
    {
        // Arrange
        var command = CreateValidCommand(order: -1);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == OrderProperty);
    }

    [Fact]
    public void Given_TagWithEmptyName_When_Validate_Then_Fails()
    {
        // Arrange
        var command = CreateValidCommand(tags: [("", "Value")]);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("Item1"));
    }

    [Fact]
    public void Given_TagWithEmptyValue_When_Validate_Then_Fails()
    {
        // Arrange
        var command = CreateValidCommand(tags: [("Key", "")]);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("Item2"));
    }
}
