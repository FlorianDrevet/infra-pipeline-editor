using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.Application.InfrastructureConfig.Queries.ListUsers;
using InfraFlowSculptor.Domain.UserAggregate;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using MapsterMapper;
using NSubstitute;
using Name = InfraFlowSculptor.Domain.UserAggregate.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.InfrastructureConfig.Queries.ListUsers;

public sealed class ListUsersQueryHandlerTests
{
    private readonly IMapper _mapper;
    private readonly ListUsersQueryHandler _sut;
    private readonly IUserRepository _userRepository;

    public ListUsersQueryHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _mapper = Substitute.For<IMapper>();
        _sut = new ListUsersQueryHandler(_userRepository, _mapper);
    }

    [Fact]
    public async Task Given_StoredUsers_When_Handle_Then_RequestsAllUsersWithCancellationToken_Async()
    {
        // Arrange
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var firstUser = User.Create(new EntraId(Guid.NewGuid()), new Name("Alice", "Liddell"));
        var secondUser = User.Create(new EntraId(Guid.NewGuid()), new Name("Bob", "Builder"));
        var users = new List<User> { firstUser, secondUser };
        var firstResult = new UserResult(firstUser.Id, "Alice", "Liddell");
        var secondResult = new UserResult(secondUser.Id, "Bob", "Builder");

        _userRepository.GetAllAsync(cancellationToken)
            .Returns(users);
        _mapper.Map<UserResult>(firstUser).Returns(firstResult);
        _mapper.Map<UserResult>(secondUser).Returns(secondResult);

        // Act
        var result = await _sut.Handle(new ListUsersQuery(), cancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Equal(firstResult, secondResult);
        await _userRepository.Received(1).GetAllAsync(cancellationToken);
    }
}