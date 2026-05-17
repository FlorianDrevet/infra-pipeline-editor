using FluentAssertions;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.SetResourceNamingTemplate;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.InfrastructureConfig.Commands.SetResourceNamingTemplate;

public sealed class SetResourceNamingTemplateCommandValidatorTests
{
    private readonly SetResourceNamingTemplateCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        var command = new SetResourceNamingTemplateCommand(
            InfrastructureConfigId.CreateUnique(), "Microsoft.KeyVault/vaults", "{name}-{env}");

        var result = _sut.Validate(command);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_EmptyResourceType_When_Validate_Then_Fails()
    {
        var command = new SetResourceNamingTemplateCommand(
            InfrastructureConfigId.CreateUnique(), "", "{name}-{env}");

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SetResourceNamingTemplateCommand.ResourceType));
    }

    [Fact]
    public void Given_ResourceTypeTooLong_When_Validate_Then_Fails()
    {
        var command = new SetResourceNamingTemplateCommand(
            InfrastructureConfigId.CreateUnique(), new string('x', 101), "{name}-{env}");

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SetResourceNamingTemplateCommand.ResourceType));
    }

    [Fact]
    public void Given_EmptyTemplate_When_Validate_Then_Fails()
    {
        var command = new SetResourceNamingTemplateCommand(
            InfrastructureConfigId.CreateUnique(), "Microsoft.KeyVault/vaults", "");

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SetResourceNamingTemplateCommand.Template));
    }

    [Fact]
    public void Given_TemplateTooLong_When_Validate_Then_Fails()
    {
        var command = new SetResourceNamingTemplateCommand(
            InfrastructureConfigId.CreateUnique(), "Microsoft.KeyVault/vaults", new string('a', 501));

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SetResourceNamingTemplateCommand.Template));
    }

    [Fact]
    public void Given_TemplateWithUnknownPlaceholder_When_Validate_Then_Fails()
    {
        var command = new SetResourceNamingTemplateCommand(
            InfrastructureConfigId.CreateUnique(), "Microsoft.KeyVault/vaults", "{unknown}-{name}");

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SetResourceNamingTemplateCommand.Template));
    }
}
