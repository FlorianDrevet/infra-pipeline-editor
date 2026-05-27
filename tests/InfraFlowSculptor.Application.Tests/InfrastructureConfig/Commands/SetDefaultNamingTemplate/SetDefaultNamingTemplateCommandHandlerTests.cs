using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.SetDefaultNamingTemplate;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.InfrastructureConfig.Commands.SetDefaultNamingTemplate;

public sealed class SetDefaultNamingTemplateCommandHandlerTests
{
    private readonly IInfrastructureConfigRepository _repository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly DomainInfrastructureConfig _config;
    private readonly SetDefaultNamingTemplateCommandHandler _sut;

    public SetDefaultNamingTemplateCommandHandlerTests()
    {
        _repository = Substitute.For<IInfrastructureConfigRepository>();
        _accessService = Substitute.For<IInfraConfigAccessService>();
        var project = Project.Create(new Name("Retail Platform"), "Provision retail assets.", UserId.CreateUnique());
        _config = DomainInfrastructureConfig.Create(new Name("primary"), project.Id);

        _sut = new SetDefaultNamingTemplateCommandHandler(_repository, _accessService);
    }

    [Fact]
    public async Task Given_ValidTemplate_When_Handle_Then_ReturnsUpdatedAsync()
    {
        // Arrange
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);
        var command = new SetDefaultNamingTemplateCommand(_config.Id, "{name}-{env}");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Updated);
        _repository.Received(1).Update(Arg.Is<DomainInfrastructureConfig>(c => c.Id == _config.Id));
    }

    [Fact]
    public async Task Given_NullTemplate_When_Handle_Then_ClearsAndReturnsUpdatedAsync()
    {
        // Arrange
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);
        var command = new SetDefaultNamingTemplateCommand(_config.Id, null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Updated);
        _repository.Received(1).Update(Arg.Any<DomainInfrastructureConfig>());
    }

    [Fact]
    public async Task Given_AccessDenied_When_Handle_Then_ReturnsErrorAsync()
    {
        // Arrange
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(Error.Unauthorized());
        var command = new SetDefaultNamingTemplateCommand(_config.Id, "{name}-{env}");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Unauthorized);
        _repository.DidNotReceive().Update(Arg.Any<DomainInfrastructureConfig>());
    }
}
