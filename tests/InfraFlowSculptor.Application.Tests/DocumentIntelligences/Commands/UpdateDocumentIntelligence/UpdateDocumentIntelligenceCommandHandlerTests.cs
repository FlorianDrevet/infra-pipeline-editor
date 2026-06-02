using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.DocumentIntelligences.Commands.UpdateDocumentIntelligence;
using InfraFlowSculptor.Application.DocumentIntelligences.Common;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.DocumentIntelligenceAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using MapsterMapper;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.DocumentIntelligences.Commands.UpdateDocumentIntelligence;

public sealed class UpdateDocumentIntelligenceCommandHandlerTests
{
    private const string UpdatedName = "doc-intel-renamed";

    private readonly IDocumentIntelligenceRepository _documentIntelligenceRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly IMapper _mapper;
    private readonly DomainInfrastructureConfig _config;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly DocumentIntelligence _existingEntity;
    private readonly UpdateDocumentIntelligenceCommand _command;
    private readonly UpdateDocumentIntelligenceCommandHandler _sut;

    public UpdateDocumentIntelligenceCommandHandlerTests()
    {
        _documentIntelligenceRepository = Substitute.For<IDocumentIntelligenceRepository>();
        _resourceGroupRepository = Substitute.For<IResourceGroupRepository>();
        _accessService = Substitute.For<IInfraConfigAccessService>();
        _mapper = Substitute.For<IMapper>();

        _config = DomainInfrastructureConfig.Create(new Name("primary"), ProjectId.CreateUnique());
        _resourceGroup = DomainResourceGroup.Create(
            new Name("rg-shared"),
            _config.Id,
            new Location(Location.LocationEnum.FranceCentral));
        _existingEntity = DocumentIntelligence.Create(
            _resourceGroup.Id,
            new Name("doc-intel-old"),
            new Location(Location.LocationEnum.FranceCentral),
            customSubDomainName: "old-subdomain");

        _command = new UpdateDocumentIntelligenceCommand(
            _existingEntity.Id,
            new Name(UpdatedName),
            new Location(Location.LocationEnum.WestEurope),
            "new-subdomain");

        _documentIntelligenceRepository.Update(Arg.Any<DocumentIntelligence>())
            .Returns(callInfo => (DocumentIntelligence)callInfo.Args()[0]);

        _sut = new UpdateDocumentIntelligenceCommandHandler(
            _documentIntelligenceRepository, _resourceGroupRepository, _accessService, _mapper);
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
        _documentIntelligenceRepository.DidNotReceive().Update(Arg.Any<DocumentIntelligence>());
        await _accessService.DidNotReceive().VerifyWriteAccessAsync(Arg.Any<Domain.InfrastructureConfigAggregate.ValueObjects.InfrastructureConfigId>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_ResourceGroupNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _documentIntelligenceRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_existingEntity);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns((DomainResourceGroup?)null);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        _documentIntelligenceRepository.DidNotReceive().Update(Arg.Any<DocumentIntelligence>());
    }

    [Fact]
    public async Task Given_WriteAccessDenied_When_Handle_Then_ReturnsErrorAndDoesNotUpdateAsync()
    {
        // Arrange
        _documentIntelligenceRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_existingEntity);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(Error.Unauthorized());

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        _documentIntelligenceRepository.DidNotReceive().Update(Arg.Any<DocumentIntelligence>());
    }

    [Fact]
    public async Task Given_WriteAccessGranted_When_Handle_Then_UpdatesEntityAndMapsResultAsync()
    {
        // Arrange
        _documentIntelligenceRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_existingEntity);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        _documentIntelligenceRepository.Received(1).Update(Arg.Is<DocumentIntelligence>(d =>
            d.Name.Value == UpdatedName));
        _mapper.Received(1).Map<DocumentIntelligenceResult>(Arg.Any<DocumentIntelligence>());
    }

    [Fact]
    public async Task Given_UpdatedEnvironmentSettings_When_Handle_Then_AppliesSettingsBeforeUpdateAsync()
    {
        // Arrange
        var commandWithSettings = _command with
        {
            EnvironmentSettings = new[]
            {
                new DocumentIntelligenceEnvironmentConfigData("prod", "S0", "Disabled", true),
            }
        };

        _documentIntelligenceRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_existingEntity);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);

        // Act
        var result = await _sut.Handle(commandWithSettings, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        _documentIntelligenceRepository.Received(1).Update(Arg.Is<DocumentIntelligence>(d =>
            d.EnvironmentSettings.Count == 1));
    }

    [Fact]
    public async Task Given_InvalidSkuInEnvironmentSettings_When_Handle_Then_ReturnsValidationErrorAsync()
    {
        // Arrange
        var commandWithBadSku = _command with
        {
            EnvironmentSettings = new[]
            {
                new DocumentIntelligenceEnvironmentConfigData("dev", "NOT_A_SKU", null, false),
            }
        };

        _documentIntelligenceRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_existingEntity);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);

        // Act
        var result = await _sut.Handle(commandWithBadSku, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
        _documentIntelligenceRepository.DidNotReceive().Update(Arg.Any<DocumentIntelligence>());
    }
}
