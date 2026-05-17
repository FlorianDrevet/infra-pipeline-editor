using FluentAssertions;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.RemoveResourceNamingTemplate;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.InfrastructureConfig.Commands.RemoveResourceNamingTemplate;

public sealed class RemoveResourceNamingTemplateCommandValidatorTests
{
    private readonly RemoveResourceNamingTemplateCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        var command = new RemoveResourceNamingTemplateCommand(
            InfrastructureConfigId.CreateUnique(), "Microsoft.KeyVault/vaults");

        var result = _sut.Validate(command);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_EmptyInfraConfigId_When_Validate_Then_Fails()
    {
        var command = new RemoveResourceNamingTemplateCommand(
            InfrastructureConfigId.Create(Guid.Empty), "Microsoft.KeyVault/vaults");

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "InfraConfigId.Value");
    }

    [Fact]
    public void Given_EmptyResourceType_When_Validate_Then_Fails()
    {
        var command = new RemoveResourceNamingTemplateCommand(
            InfrastructureConfigId.CreateUnique(), "");

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RemoveResourceNamingTemplateCommand.ResourceType));
    }

    [Fact]
    public void Given_ResourceTypeTooLong_When_Validate_Then_Fails()
    {
        var command = new RemoveResourceNamingTemplateCommand(
            InfrastructureConfigId.CreateUnique(), new string('x', 101));

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RemoveResourceNamingTemplateCommand.ResourceType));
    }
}
