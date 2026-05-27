using FluentAssertions;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.SetInfraConfigTags;

namespace InfraFlowSculptor.Application.Tests.InfrastructureConfig.Commands.SetInfraConfigTags;

public sealed class SetInfraConfigTagsCommandValidatorTests
{
    private readonly SetInfraConfigTagsCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        var command = new SetInfraConfigTagsCommand(Guid.NewGuid(),
            [("Environment", "Production")]);

        var result = _sut.Validate(command);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_EmptyInfraConfigId_When_Validate_Then_Fails()
    {
        var command = new SetInfraConfigTagsCommand(Guid.Empty,
            [("Environment", "Production")]);

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SetInfraConfigTagsCommand.InfraConfigId));
    }

    [Fact]
    public void Given_TooManyTags_When_Validate_Then_Fails()
    {
        var tags = Enumerable.Range(0, 16)
            .Select(i => ($"Key{i}", $"Value{i}"))
            .ToList();

        var command = new SetInfraConfigTagsCommand(Guid.NewGuid(), tags);

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SetInfraConfigTagsCommand.Tags));
    }

    [Fact]
    public void Given_TagWithEmptyName_When_Validate_Then_Fails()
    {
        var command = new SetInfraConfigTagsCommand(Guid.NewGuid(),
            [("", "Value")]);

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("name", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Given_TagWithEmptyValue_When_Validate_Then_Fails()
    {
        var command = new SetInfraConfigTagsCommand(Guid.NewGuid(),
            [("Key", "")]);

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("value", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Given_TagNameTooLong_When_Validate_Then_Fails()
    {
        var command = new SetInfraConfigTagsCommand(Guid.NewGuid(),
            [(new string('k', 513), "Value")]);

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("name", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Given_TagValueTooLong_When_Validate_Then_Fails()
    {
        var command = new SetInfraConfigTagsCommand(Guid.NewGuid(),
            [("Key", new string('v', 257))]);

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("value", StringComparison.OrdinalIgnoreCase));
    }
}
