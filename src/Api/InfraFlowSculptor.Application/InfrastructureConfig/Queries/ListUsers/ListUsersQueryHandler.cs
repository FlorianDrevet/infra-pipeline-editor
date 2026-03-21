using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Queries.ListUsers;

/// <summary>
/// Handles the <see cref="ListUsersQuery"/> request
/// and returns all registered users.
/// </summary>
public sealed class ListUsersQueryHandler(
    IUserRepository userRepository,
    IMapper mapper)
    : IRequestHandler<ListUsersQuery, ErrorOr<List<UserResult>>>
{
    /// <inheritdoc />
    public async Task<ErrorOr<List<UserResult>>> Handle(
        ListUsersQuery request,
        CancellationToken cancellationToken)
    {
        var users = await userRepository.GetAllAsync(cancellationToken);
        return users.Select(u => mapper.Map<UserResult>(u)).ToList();
    }
}
