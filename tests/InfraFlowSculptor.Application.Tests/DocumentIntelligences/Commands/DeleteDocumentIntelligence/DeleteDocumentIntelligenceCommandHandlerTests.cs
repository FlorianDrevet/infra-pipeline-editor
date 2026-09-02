using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.DocumentIntelligences.Commands.DeleteDocumentIntelligence;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.DocumentIntelligenceAggregate;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.DocumentIntelligences.Commands.DeleteDocumentIntelligence;

public sealed class DeleteDocumentIntelligenceCommandHandlerTests
{
    private readonly IDocumentIntelligenceRepository _documentIntelligenceRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly DomainInfrastructureConfig _config;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly DocumentIntelligence _documentIntelligence;
    private readonly DeleteDocumentIntelligenceCommand _command;
    private readonly DeleteDocumentIntelligenceCommandHandler _sut;

    public DeleteDocumentIntelligenceCommandHandlerTests()
    {
        _documentIntelligenceRepository = Substitute.For<IDocumentIntelligenceRepository>();
        _resourceGroupRepository = Substitute.For<IResourceGroupRepository>();
        _accessService = Substitute.For<IInfraConfigAccessService>();

        _config = DomainInfrastructureConfig.Create(new Name("primary"), ProjectId.CreateUnique());
        _resourceGroup = DomainResourceGroup.Create(
            new Name("rg-shared"),
            _config.Id,
            new Location(Location.LocationEnum.FranceCentral));
        _documentIntelligence = DocumentIntelligence.Create(
            _resourceGroup.Id,
            new Name("doc-intel-shared"),
            new Location(Location.LocationEnum.FranceCentral),
            customSubDomainName: null);

        _command = new DeleteDocumentIntelligenceCommand(_documentIntelligence.Id);

        _sut = new DeleteDocumentIntelligenceCommandHandler(
            _documentIntelligenceRepository, _resourceGroupRepository, _accessService);
    }

    [Fact]
    public async Task Given_DocumentIntelligenceNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _documentIntelligenceRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns((DocumentIntelligence?)null);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        await _accessService.DidNotReceive().VerifyWriteAccessAsync(Arg.Any<InfrastructureConfigId>(), Arg.Any<CancellationToken>());
        await _documentIntelligenceRepository.DidNotReceive().DeleteAsync(Arg.Any<AzureResourceId>());
    }

    [Fact]
    public async Task Given_ResourceGroupNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _documentIntelligenceRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_documentIntelligence);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns((DomainResourceGroup?)null);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        await _documentIntelligenceRepository.DidNotReceive().DeleteAsync(Arg.Any<AzureResourceId>());
    }

    [Fact]
    public async Task Given_WriteAccessDenied_When_Handle_Then_ReturnsErrorAndDoesNotDeleteAsync()
    {
        // Arrange
        _documentIntelligenceRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_documentIntelligence);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(Error.Forbidden());

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Forbidden);
        await _documentIntelligenceRepository.DidNotReceive().DeleteAsync(Arg.Any<AzureResourceId>());
    }

    [Fact]
    public async Task Given_WriteAccessGranted_When_Handle_Then_DeletesDocumentIntelligenceAsync()
    {
        // Arrange
        _documentIntelligenceRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_documentIntelligence);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);
        _documentIntelligenceRepository.DeleteAsync(_documentIntelligence.Id).Returns(true);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Deleted);
        await _documentIntelligenceRepository.Received(1).DeleteAsync(_documentIntelligence.Id);
    }
}
