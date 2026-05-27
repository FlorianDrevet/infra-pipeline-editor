using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.SetInheritance;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.InfrastructureConfig.Commands.SetInheritance;

public sealed class SetInheritanceCommandHandlerTests
{
    private readonly IInfrastructureConfigRepository _repository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly DomainInfrastructureConfig _config;
    private readonly SetInheritanceCommandHandler _sut;

    public SetInheritanceCommandHandlerTests()
    {
        _repository = Substitute.For<IInfrastructureConfigRepository>();
        _accessService = Substitute.For<IInfraConfigAccessService>();
        var project = Project.Create(new Name("Retail Platform"), "Provision retail assets.", UserId.CreateUnique());
        _config = DomainInfrastructureConfig.Create(new Name("primary"), project.Id);

        _sut = new SetInheritanceCommandHandler(_repository, _accessService);
    }

    [Fact]
    public async Task Given_ValidFlag_When_Handle_Then_ReturnsSuccessAsync()
    {
        // Arrange
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);
        _repository.GetByIdAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);
        var command = new SetInheritanceCommand(_config.Id, true);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Success);
        _repository.Received(1).Update(Arg.Is<DomainInfrastructureConfig>(c => c.Id == _config.Id));
    }

    [Fact]
    public async Task Given_AccessDenied_When_Handle_Then_ReturnsErrorAsync()
    {
        // Arrange
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(Error.Unauthorized());
        var command = new SetInheritanceCommand(_config.Id, true);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Unauthorized);
        _repository.DidNotReceive().Update(Arg.Any<DomainInfrastructureConfig>());
    }

    [Fact]
    public async Task Given_ConfigNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);
        _repository.GetByIdAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns((DomainInfrastructureConfig?)null);
        var command = new SetInheritanceCommand(_config.Id, false);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        _repository.DidNotReceive().Update(Arg.Any<DomainInfrastructureConfig>());
    }
}
