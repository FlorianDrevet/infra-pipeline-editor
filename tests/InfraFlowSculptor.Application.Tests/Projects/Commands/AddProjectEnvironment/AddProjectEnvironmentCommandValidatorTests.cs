using FluentAssertions;
using InfraFlowSculptor.Application.Projects.Commands.AddProjectEnvironment;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.Projects.Commands.AddProjectEnvironment;

public sealed class AddProjectEnvironmentCommandValidatorTests
{
    private readonly AddProjectEnvironmentCommandValidator _sut = new();

    private static AddProjectEnvironmentCommand ValidCommand() => new(
        ProjectId.CreateUnique(),
        Name: "Development",
        ShortName: "dev",
        Prefix: "dev-",
        Suffix: "-01",
        Location: "francecentral",
        SubscriptionId: Guid.NewGuid(),
        Order: 0,
        RequiresApproval: false,
        AzureResourceManagerConnection: null,
        Tags: [("Environment", "dev")]);

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        var result = _sut.Validate(ValidCommand());

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_EmptyName_When_Validate_Then_FailsOnName()
    {
        var command = ValidCommand() with { Name = "" };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AddProjectEnvironmentCommand.Name));
    }

    [Fact]
    public void Given_NameTooLong_When_Validate_Then_FailsOnName()
    {
        var command = ValidCommand() with { Name = new string('a', 101) };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AddProjectEnvironmentCommand.Name));
    }

    [Fact]
    public void Given_NullPrefix_When_Validate_Then_FailsOnPrefix()
    {
        var command = ValidCommand() with { Prefix = null! };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AddProjectEnvironmentCommand.Prefix));
    }

    [Fact]
    public void Given_PrefixTooLong_When_Validate_Then_FailsOnPrefix()
    {
        var command = ValidCommand() with { Prefix = new string('p', 51) };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AddProjectEnvironmentCommand.Prefix));
    }

    [Fact]
    public void Given_NullSuffix_When_Validate_Then_FailsOnSuffix()
    {
        var command = ValidCommand() with { Suffix = null! };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AddProjectEnvironmentCommand.Suffix));
    }

    [Fact]
    public void Given_SuffixTooLong_When_Validate_Then_FailsOnSuffix()
    {
        var command = ValidCommand() with { Suffix = new string('s', 51) };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AddProjectEnvironmentCommand.Suffix));
    }

    [Fact]
    public void Given_EmptyLocation_When_Validate_Then_FailsOnLocation()
    {
        var command = ValidCommand() with { Location = "" };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AddProjectEnvironmentCommand.Location));
    }

    [Fact]
    public void Given_NegativeOrder_When_Validate_Then_FailsOnOrder()
    {
        var command = ValidCommand() with { Order = -1 };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AddProjectEnvironmentCommand.Order));
    }

    [Fact]
    public void Given_TagWithEmptyName_When_Validate_Then_FailsOnTagName()
    {
        var command = ValidCommand() with { Tags = [("", "value")] };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("Item1"));
    }

    [Fact]
    public void Given_TagWithEmptyValue_When_Validate_Then_FailsOnTagValue()
    {
        var command = ValidCommand() with { Tags = [("Key", "")] };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("Item2"));
    }
}
