using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Queries.ListUsers;

/// <summary>Query to retrieve all registered users.</summary>
public record ListUsersQuery : IQuery<List<UserResult>>;
