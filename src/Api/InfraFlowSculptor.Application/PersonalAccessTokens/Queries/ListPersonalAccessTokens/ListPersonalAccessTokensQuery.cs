using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.PersonalAccessTokens.Common;

namespace InfraFlowSculptor.Application.PersonalAccessTokens.Queries.ListPersonalAccessTokens;

/// <summary>
/// Query to list all personal access tokens belonging to the current authenticated user.
/// </summary>
public record ListPersonalAccessTokensQuery() : IQuery<List<PersonalAccessTokenResult>>;
