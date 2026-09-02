using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.AddCrossConfigReference;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.InfrastructureConfig.Commands.AddCrossConfigReference;

public sealed class AddCrossConfigReferenceCommandHandlerTests
{
    private readonly IInfraConfigAccessService _accessService;
    private readonly IInfrastructureConfigRepository _infraConfigRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly DomainInfrastructureConfig _sourceConfig;
    private readonly DomainInfrastructureConfig _targetConfig;
    private readonly DomainResourceGroup _targetResourceGroup;
    private readonly AzureResourceId _targetResourceId;
    private readonly AddCrossConfigReferenceCommand _command;
    private readonly AddCrossConfigReferenceCommandHandler _sut;

    public AddCrossConfigReferenceCommandHandlerTests()
    {
        _accessService = Substitute.For<IInfraConfigAccessService>();
        _infraConfigRepository = Substitute.For<IInfrastructureConfigRepository>();
        _resourceGroupRepository = Substitute.For<IResourceGroupRepository>();

        var projectId = ProjectId.CreateUnique();
        _sourceConfig = DomainInfrastructureConfig.Create(new Name("source"), projectId);
        _targetConfig = DomainInfrastructureConfig.Create(new Name("target"), projectId);
        _targetResourceGroup = DomainResourceGroup.Create(
            new Name("rg-target"),
            _targetConfig.Id,
            new Location(Location.LocationEnum.FranceCentral));
        _targetResourceId = AzureResourceId.CreateUnique();
        _command = new AddCrossConfigReferenceCommand(
            _sourceConfig.Id.Value,
            _targetResourceId.Value);
        _sut = new AddCrossConfigReferenceCommandHandler(
            _accessService, _infraConfigRepository, _resourceGroupRepository);
    }

    [Fact]
    public async Task Given_AccessDenied_When_Handle_Then_ReturnsUnauthorizedAsync()
    {
        // Arrange
        _accessService.VerifyWriteAccessAsync(Arg.Any<InfrastructureConfigId>(), Arg.Any<CancellationToken>())
            .Returns(Error.Unauthorized());

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task Given_TargetResourceGroupNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _accessService.VerifyWriteAccessAsync(Arg.Any<InfrastructureConfigId>(), Arg.Any<CancellationToken>())
            .Returns(_sourceConfig);
        _resourceGroupRepository.GetByContainedResourceIdAsync(Arg.Any<AzureResourceId>(), Arg.Any<CancellationToken>())
            .Returns((DomainResourceGroup?)null);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Given_TargetConfigNotInSameProject_When_Handle_Then_ReturnsErrorAsync()
    {
        // Arrange
        var otherProjectConfig = DomainInfrastructureConfig.Create(
            new Name("other"), ProjectId.CreateUnique());
        _accessService.VerifyWriteAccessAsync(Arg.Any<InfrastructureConfigId>(), Arg.Any<CancellationToken>())
            .Returns(_sourceConfig);
        _resourceGroupRepository.GetByContainedResourceIdAsync(Arg.Any<AzureResourceId>(), Arg.Any<CancellationToken>())
            .Returns(_targetResourceGroup);
        _infraConfigRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(otherProjectConfig);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task Given_ValidCrossConfigReference_When_Handle_Then_AddsReferenceAndReturnsResultAsync()
    {
        // Arrange
        _accessService.VerifyWriteAccessAsync(Arg.Any<InfrastructureConfigId>(), Arg.Any<CancellationToken>())
            .Returns(_sourceConfig);
        _resourceGroupRepository.GetByContainedResourceIdAsync(Arg.Any<AzureResourceId>(), Arg.Any<CancellationToken>())
            .Returns(_targetResourceGroup);
        _infraConfigRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_targetConfig);
        _infraConfigRepository.GetByIdWithMembersAsync(Arg.Any<InfrastructureConfigId>(), Arg.Any<CancellationToken>())
            .Returns(_sourceConfig);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.TargetConfigId.Should().Be(_targetConfig.Id.Value);
        result.Value.TargetResourceId.Should().Be(_targetResourceId.Value);
        _infraConfigRepository.Received(1).Update(Arg.Any<DomainInfrastructureConfig>());
    }

    [Fact]
    public async Task Given_DuplicateCrossConfigReference_When_Handle_Then_ReturnsConflictAsync()
    {
        // Arrange
        var loadedConfigWithReferences = DomainInfrastructureConfig.Create(new Name("source-loaded"), _sourceConfig.ProjectId);
        var duplicateTargetConfig = DomainInfrastructureConfig.Create(new Name("target-loaded"), _sourceConfig.ProjectId);
        loadedConfigWithReferences.AddCrossConfigReference(duplicateTargetConfig.Id, _targetResourceId);

        _accessService.VerifyWriteAccessAsync(Arg.Any<InfrastructureConfigId>(), Arg.Any<CancellationToken>())
            .Returns(_sourceConfig);
        _resourceGroupRepository.GetByContainedResourceIdAsync(Arg.Any<AzureResourceId>(), Arg.Any<CancellationToken>())
            .Returns(_targetResourceGroup);
        _infraConfigRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_targetConfig);
        _infraConfigRepository.GetByIdWithMembersAsync(Arg.Any<InfrastructureConfigId>(), Arg.Any<CancellationToken>())
            .Returns(loadedConfigWithReferences);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Conflict);
        result.FirstError.Code.Should().Be("InfrastructureConfig.DuplicateCrossConfigReference");
        _infraConfigRepository.DidNotReceive().Update(Arg.Any<DomainInfrastructureConfig>());
    }
}
