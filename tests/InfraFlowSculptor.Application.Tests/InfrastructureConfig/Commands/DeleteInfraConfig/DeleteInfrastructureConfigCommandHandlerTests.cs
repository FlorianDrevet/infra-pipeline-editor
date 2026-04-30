using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.DeleteInfraConfig;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.InfrastructureConfig.Commands.DeleteInfraConfig;

public sealed class DeleteInfrastructureConfigCommandHandlerTests
{
    private const string ConfigName = "primary";

    private readonly IInfrastructureConfigRepository _repository;
    private readonly IProjectAccessService _accessService;
    private readonly DomainInfrastructureConfig _config;
    private readonly Project _project;
    private readonly DeleteInfrastructureConfigCommandHandler _sut;

    public DeleteInfrastructureConfigCommandHandlerTests()
    {
        _repository = Substitute.For<IInfrastructureConfigRepository>();
        _accessService = Substitute.For<IProjectAccessService>();
        _project = Project.Create(new Name("RetailApi"), null, UserId.CreateUnique());
        _config = DomainInfrastructureConfig.Create(new Name(ConfigName), _project.Id);
        _sut = new DeleteInfrastructureConfigCommandHandler(_repository, _accessService);
    }

    [Fact]
    public async Task Given_ConfigNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _repository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns((DomainInfrastructureConfig?)null);
        var command = new DeleteInfrastructureConfigCommand(_config.Id);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        await _accessService.DidNotReceive().VerifyOwnerAccessAsync(Arg.Any<ProjectId>(), Arg.Any<CancellationToken>());
        await _repository.DidNotReceive().DeleteAsync(Arg.Any<InfrastructureConfigId>());
    }

    [Fact]
    public async Task Given_OwnerAccessDenied_When_Handle_Then_ReturnsErrorAndDoesNotDeleteAsync()
    {
        // Arrange
        _repository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_config);
        _accessService.VerifyOwnerAccessAsync(_project.Id, Arg.Any<CancellationToken>())
            .Returns(Errors.Project.ForbiddenError());
        var command = new DeleteInfrastructureConfigCommand(_config.Id);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Forbidden);
        await _repository.DidNotReceive().DeleteAsync(Arg.Any<InfrastructureConfigId>());
    }

    [Fact]
    public async Task Given_OwnerAccessGranted_When_Handle_Then_DeletesConfigAsync()
    {
        // Arrange
        _repository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_config);
        _accessService.VerifyOwnerAccessAsync(_project.Id, Arg.Any<CancellationToken>())
            .Returns(_project);
        _repository.DeleteAsync(_config.Id).Returns(true);
        var command = new DeleteInfrastructureConfigCommand(_config.Id);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Deleted);
        await _repository.Received(1).DeleteAsync(_config.Id);
    }
}
