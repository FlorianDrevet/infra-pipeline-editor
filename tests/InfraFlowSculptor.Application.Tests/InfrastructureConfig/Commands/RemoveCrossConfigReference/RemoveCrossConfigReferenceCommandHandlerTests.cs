using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.RemoveCrossConfigReference;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.InfrastructureConfig.Commands.RemoveCrossConfigReference;

public sealed class RemoveCrossConfigReferenceCommandHandlerTests
{
    private readonly IInfraConfigAccessService _accessService;
    private readonly IInfrastructureConfigRepository _infraConfigRepository;
    private readonly DomainInfrastructureConfig _config;
    private readonly RemoveCrossConfigReferenceCommand _command;
    private readonly RemoveCrossConfigReferenceCommandHandler _sut;

    public RemoveCrossConfigReferenceCommandHandlerTests()
    {
        _accessService = Substitute.For<IInfraConfigAccessService>();
        _infraConfigRepository = Substitute.For<IInfrastructureConfigRepository>();
        _config = DomainInfrastructureConfig.Create(new Name("primary"), ProjectId.CreateUnique());
        _command = new RemoveCrossConfigReferenceCommand(_config.Id.Value, Guid.NewGuid());
        _sut = new RemoveCrossConfigReferenceCommandHandler(_accessService, _infraConfigRepository);
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
    public async Task Given_ReferenceNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange — config has no cross-config references
        _accessService.VerifyWriteAccessAsync(Arg.Any<InfrastructureConfigId>(), Arg.Any<CancellationToken>())
            .Returns(_config);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Given_ValidRequest_When_Handle_Then_ReturnsDeletedAsync()
    {
        // Arrange — add a cross-config reference to the config, then remove it
        var referenceId = Guid.NewGuid();

        // We need to add a reference first. Use reflection or domain method.
        // The config.AddCrossConfigReference requires target config id + resource id.
        // Instead we just test that when the domain method succeeds, we get Deleted.
        // We'll create a fresh config with a reference.
        var targetConfigId = InfrastructureConfigId.CreateUnique();
        var targetResourceId = Domain.Common.BaseModels.ValueObjects.AzureResourceId.CreateUnique();
        _config.AddCrossConfigReference(targetConfigId, targetResourceId);
        var existingRef = _config.CrossConfigReferences.First();
        var command = new RemoveCrossConfigReferenceCommand(_config.Id.Value, existingRef.Id.Value);

        _accessService.VerifyWriteAccessAsync(Arg.Any<InfrastructureConfigId>(), Arg.Any<CancellationToken>())
            .Returns(_config);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Deleted);
        _infraConfigRepository.Received(1).Update(Arg.Any<DomainInfrastructureConfig>());
    }
}
