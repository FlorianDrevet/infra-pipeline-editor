using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.SetResourceAbbreviationOverride;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using MapsterMapper;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.InfrastructureConfig.Commands.SetResourceAbbreviationOverride;

public sealed class SetResourceAbbreviationOverrideCommandHandlerTests
{
    private readonly IInfrastructureConfigRepository _repository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly IMapper _mapper;
    private readonly DomainInfrastructureConfig _config;
    private readonly SetResourceAbbreviationOverrideCommandHandler _sut;

    public SetResourceAbbreviationOverrideCommandHandlerTests()
    {
        _repository = Substitute.For<IInfrastructureConfigRepository>();
        _accessService = Substitute.For<IInfraConfigAccessService>();
        _mapper = Substitute.For<IMapper>();
        var project = Project.Create(new Name("Retail Platform"), "Provision retail assets.", UserId.CreateUnique());
        _config = DomainInfrastructureConfig.Create(new Name("primary"), project.Id);

        _sut = new SetResourceAbbreviationOverrideCommandHandler(_repository, _accessService, _mapper);
    }

    [Fact]
    public async Task Given_ValidAbbreviation_When_Handle_Then_ReturnsMappedResultAsync()
    {
        // Arrange
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);
        _repository.GetByIdWithNamingTemplatesAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);
        var expectedResult = new ResourceAbbreviationOverrideResult(
            ResourceAbbreviationOverrideId.CreateUnique(), "KeyVault", "kv");
        _mapper.Map<ResourceAbbreviationOverrideResult>(Arg.Any<object>())
            .Returns(expectedResult);
        var command = new SetResourceAbbreviationOverrideCommand(_config.Id, "KeyVault", "kv");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeSameAs(expectedResult);
        _repository.Received(1).Update(Arg.Is<DomainInfrastructureConfig>(c => c.Id == _config.Id));
    }

    [Fact]
    public async Task Given_AccessDenied_When_Handle_Then_ReturnsErrorAsync()
    {
        // Arrange
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(Error.Unauthorized());
        var command = new SetResourceAbbreviationOverrideCommand(_config.Id, "KeyVault", "kv");

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
        _repository.GetByIdWithNamingTemplatesAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns((DomainInfrastructureConfig?)null);
        var command = new SetResourceAbbreviationOverrideCommand(_config.Id, "KeyVault", "kv");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        _repository.DidNotReceive().Update(Arg.Any<DomainInfrastructureConfig>());
    }
}
