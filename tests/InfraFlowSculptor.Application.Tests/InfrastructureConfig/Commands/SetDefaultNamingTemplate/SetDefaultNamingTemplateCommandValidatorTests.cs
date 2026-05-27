using FluentAssertions;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.SetDefaultNamingTemplate;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.InfrastructureConfig.Commands.SetDefaultNamingTemplate;

public sealed class SetDefaultNamingTemplateCommandValidatorTests
{
    private readonly SetDefaultNamingTemplateCommandValidator _sut = new();

    [Fact]
    public void Given_NullTemplate_When_Validate_Then_Succeeds()
    {
        var command = new SetDefaultNamingTemplateCommand(InfrastructureConfigId.CreateUnique(), null);

        var result = _sut.Validate(command);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_ValidTemplate_When_Validate_Then_Succeeds()
    {
        var command = new SetDefaultNamingTemplateCommand(
            InfrastructureConfigId.CreateUnique(), "{name}-{env}");

        var result = _sut.Validate(command);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_TemplateTooLong_When_Validate_Then_Fails()
    {
        var command = new SetDefaultNamingTemplateCommand(
            InfrastructureConfigId.CreateUnique(), new string('a', 501));

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SetDefaultNamingTemplateCommand.Template));
    }

    [Fact]
    public void Given_TemplateWithInvalidStaticChars_When_Validate_Then_Fails()
    {
        var command = new SetDefaultNamingTemplateCommand(
            InfrastructureConfigId.CreateUnique(), "{name}@{env}");

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SetDefaultNamingTemplateCommand.Template));
    }

    [Fact]
    public void Given_TemplateWithUnknownPlaceholder_When_Validate_Then_Fails()
    {
        var command = new SetDefaultNamingTemplateCommand(
            InfrastructureConfigId.CreateUnique(), "{name}-{unknown}");

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SetDefaultNamingTemplateCommand.Template));
    }
}
