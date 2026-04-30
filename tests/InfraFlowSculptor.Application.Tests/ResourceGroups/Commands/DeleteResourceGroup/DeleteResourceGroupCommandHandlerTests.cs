using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.ResourceGroups.Commands.DeleteResourceGroup;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.ResourceGroups.Commands.DeleteResourceGroup;

public sealed class DeleteResourceGroupCommandHandlerTests
{
    private readonly IResourceGroupRepository _repository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly DomainInfrastructureConfig _config;
    private readonly DeleteResourceGroupCommandHandler _sut;

    public DeleteResourceGroupCommandHandlerTests()
    {
        _repository = Substitute.For<IResourceGroupRepository>();
        _accessService = Substitute.For<IInfraConfigAccessService>();
        _config = DomainInfrastructureConfig.Create(new Name("primary"), ProjectId.CreateUnique());
        _resourceGroup = DomainResourceGroup.Create(
            new Name("rg-shared"),
            _config.Id,
            new Location(Location.LocationEnum.FranceCentral));
        _sut = new DeleteResourceGroupCommandHandler(_repository, _accessService);
    }

    [Fact]
    public async Task Given_ResourceGroupNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _repository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns((DomainResourceGroup?)null);
        var command = new DeleteResourceGroupCommand(_resourceGroup.Id);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        await _accessService.DidNotReceive().VerifyWriteAccessAsync(Arg.Any<InfrastructureConfigId>(), Arg.Any<CancellationToken>());
        await _repository.DidNotReceive().DeleteAsync(Arg.Any<ResourceGroupId>());
    }

    [Fact]
    public async Task Given_WriteAccessDenied_When_Handle_Then_ReturnsErrorAndDoesNotDeleteAsync()
    {
        // Arrange
        _repository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(Errors.InfrastructureConfig.ForbiddenError());
        var command = new DeleteResourceGroupCommand(_resourceGroup.Id);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Forbidden);
        await _repository.DidNotReceive().DeleteAsync(Arg.Any<ResourceGroupId>());
    }

    [Fact]
    public async Task Given_WriteAccessGranted_When_Handle_Then_DeletesResourceGroupAsync()
    {
        // Arrange
        _repository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);
        _repository.DeleteAsync(_resourceGroup.Id).Returns(true);
        var command = new DeleteResourceGroupCommand(_resourceGroup.Id);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Deleted);
        await _repository.Received(1).DeleteAsync(_resourceGroup.Id);
    }
}
