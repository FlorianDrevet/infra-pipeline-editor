using ErrorOr;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Queries.ListUsers;

/// <summary>Query to retrieve all registered users.</summary>
public record ListUsersQuery : IRequest<ErrorOr<List<UserResult>>>;
