using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.DocumentIntelligences.Common;
using InfraFlowSculptor.Application.DocumentIntelligences.Queries;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.DocumentIntelligenceAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using MapsterMapper;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.DocumentIntelligences.Queries;

public sealed class GetDocumentIntelligenceQueryHandlerTests
{
    private readonly IDocumentIntelligenceRepository _documentIntelligenceRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly IMapper _mapper;
    private readonly DomainInfrastructureConfig _config;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly DocumentIntelligence _documentIntelligence;
    private readonly GetDocumentIntelligenceQuery _query;
    private readonly GetDocumentIntelligenceQueryHandler _sut;

    public GetDocumentIntelligenceQueryHandlerTests()
    {
        _documentIntelligenceRepository = Substitute.For<IDocumentIntelligenceRepository>();
        _accessService = Substitute.For<IInfraConfigAccessService>();
        _mapper = Substitute.For<IMapper>();

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

        // Simulate EF Core navigation property loading (ResourceGroup is populated by EF Include)
        typeof(Domain.Common.BaseModels.AzureResource)
            .GetProperty(nameof(Domain.Common.BaseModels.AzureResource.ResourceGroup))!
            .SetValue(_documentIntelligence, _resourceGroup);

        _query = new GetDocumentIntelligenceQuery(_documentIntelligence.Id);
        _sut = new GetDocumentIntelligenceQueryHandler(
            _documentIntelligenceRepository, _accessService, _mapper);
    }

    [Fact]
    public async Task Given_DocumentIntelligenceNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _documentIntelligenceRepository.GetByIdReadOnlyAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns((DocumentIntelligence?)null);

        // Act
        var result = await _sut.Handle(_query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        await _accessService.DidNotReceive().VerifyReadAccessAsync(Arg.Any<Domain.InfrastructureConfigAggregate.ValueObjects.InfrastructureConfigId>(), Arg.Any<CancellationToken>());
        _mapper.DidNotReceive().Map<DocumentIntelligenceResult>(Arg.Any<DocumentIntelligence>());
    }

    [Fact]
    public async Task Given_ReadAccessDenied_When_Handle_Then_ReturnsNotFoundToHideExistenceAsync()
    {
        // Arrange
        _documentIntelligenceRepository.GetByIdReadOnlyAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_documentIntelligence);
        _accessService.VerifyReadAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(Errors.InfrastructureConfig.ForbiddenError());

        // Act
        var result = await _sut.Handle(_query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        _mapper.DidNotReceive().Map<DocumentIntelligenceResult>(Arg.Any<DocumentIntelligence>());
    }

    [Fact]
    public async Task Given_ReadAccessGranted_When_Handle_Then_MapsAndReturnsResultAsync()
    {
        // Arrange
        _documentIntelligenceRepository.GetByIdReadOnlyAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_documentIntelligence);
        _accessService.VerifyReadAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);

        // Act
        var result = await _sut.Handle(_query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        _mapper.Received(1).Map<DocumentIntelligenceResult>(_documentIntelligence);
    }
}
