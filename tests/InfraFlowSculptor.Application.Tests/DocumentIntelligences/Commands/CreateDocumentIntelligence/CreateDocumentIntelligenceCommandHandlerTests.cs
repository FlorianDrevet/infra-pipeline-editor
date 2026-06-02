using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.DocumentIntelligences.Commands.CreateDocumentIntelligence;
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

namespace InfraFlowSculptor.Application.Tests.DocumentIntelligences.Commands.CreateDocumentIntelligence;

public sealed class CreateDocumentIntelligenceCommandHandlerTests
{
    private const string ResourceName = "doc-intel-shared";
    private const string CustomSubDomainName = "my-docintel";

    private readonly IDocumentIntelligenceRepository _documentIntelligenceRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly IMapper _mapper;
    private readonly DomainInfrastructureConfig _config;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly CreateDocumentIntelligenceCommand _command;
    private readonly CreateDocumentIntelligenceCommandHandler _sut;

    public CreateDocumentIntelligenceCommandHandlerTests()
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

        _command = new CreateDocumentIntelligenceCommand(
            _resourceGroup.Id,
            new Name(ResourceName),
            new Location(Location.LocationEnum.FranceCentral),
            CustomSubDomainName);

        _documentIntelligenceRepository.Add(Arg.Any<DocumentIntelligence>())
            .Returns(callInfo => (DocumentIntelligence)callInfo.Args()[0]);

        _sut = new CreateDocumentIntelligenceCommandHandler(
            _documentIntelligenceRepository, _resourceGroupRepository, _accessService, _mapper);
    }

    [Fact]
    public async Task Given_ResourceGroupNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns((DomainResourceGroup?)null);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        _documentIntelligenceRepository.DidNotReceive().Add(Arg.Any<DocumentIntelligence>());
        await _accessService.DidNotReceive().VerifyWriteAccessAsync(Arg.Any<Domain.InfrastructureConfigAggregate.ValueObjects.InfrastructureConfigId>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_WriteAccessDenied_When_Handle_Then_ReturnsErrorAndDoesNotPersistAsync()
    {
        // Arrange
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(Error.Unauthorized());

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Unauthorized);
        _documentIntelligenceRepository.DidNotReceive().Add(Arg.Any<DocumentIntelligence>());
    }

    [Fact]
    public async Task Given_WriteAccessGranted_When_Handle_Then_PersistsDocumentIntelligenceAndMapsResultAsync()
    {
        // Arrange
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        _documentIntelligenceRepository.Received(1).Add(Arg.Is<DocumentIntelligence>(d =>
            d.ResourceGroupId == _resourceGroup.Id &&
            d.Name.Value == ResourceName &&
            d.CustomSubDomainName == CustomSubDomainName));
        _mapper.Received(1).Map<DocumentIntelligenceResult>(Arg.Any<DocumentIntelligence>());
    }

    [Fact]
    public async Task Given_ValidEnvironmentSettings_When_Handle_Then_ParsesSkuAndPersistsAsync()
    {
        // Arrange
        var commandWithSettings = _command with
        {
            EnvironmentSettings = new[]
            {
                new DocumentIntelligenceEnvironmentConfigData("dev", "F0", "Enabled", false),
                new DocumentIntelligenceEnvironmentConfigData("prod", "S0", "Disabled", true),
            }
        };

        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);

        // Act
        var result = await _sut.Handle(commandWithSettings, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        _documentIntelligenceRepository.Received(1).Add(Arg.Is<DocumentIntelligence>(d =>
            d.EnvironmentSettings.Count == 2));
    }

    [Fact]
    public async Task Given_InvalidSku_When_Handle_Then_ReturnsValidationErrorAsync()
    {
        // Arrange
        var commandWithBadSku = _command with
        {
            EnvironmentSettings = new[]
            {
                new DocumentIntelligenceEnvironmentConfigData("dev", "INVALID_SKU", null, false),
            }
        };

        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);

        // Act
        var result = await _sut.Handle(commandWithBadSku, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
        _documentIntelligenceRepository.DidNotReceive().Add(Arg.Any<DocumentIntelligence>());
    }

    [Fact]
    public async Task Given_IsExistingTrue_When_Handle_Then_PersistsWithIsExistingFlagAsync()
    {
        // Arrange
        var commandExisting = _command with { IsExisting = true };

        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);

        // Act
        var result = await _sut.Handle(commandExisting, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        _documentIntelligenceRepository.Received(1).Add(Arg.Is<DocumentIntelligence>(d =>
            d.IsExisting == true));
    }
}
