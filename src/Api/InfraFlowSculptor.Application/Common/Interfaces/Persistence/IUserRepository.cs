using InfraFlowSculptor.Domain.UserAggregate;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using InfraFlowSculptor.Application.Common.Interfaces;

namespace InfraFlowSculptor.Application.Common.Interfaces.Persistence;

/// <summary>Repository abstraction for the <see cref="User"/> aggregate.</summary>
public interface IUserRepository : IRepository<User>
{
    /// <summary>
    /// Retrieves an existing user by Entra ID or creates a new one if none exists.
    /// </summary>
    /// <param name="entraId">The Azure Entra Object ID of the user.</param>
    /// <param name="firstName">The first name to use when creating a new user.</param>
    /// <param name="lastName">The last name to use when creating a new user.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The existing or newly created <see cref="User"/>.</returns>
    Task<User> GetOrCreateByEntraIdAsync(string entraId, string firstName, string lastName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all users whose identifiers are in the specified list.
    /// </summary>
    /// <param name="ids">The list of user identifiers to look up.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A list of matching <see cref="User"/> entities.</returns>
    Task<List<User>> GetByIdsAsync(IReadOnlyList<UserId> ids, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all registered users.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A list of all <see cref="User"/> entities.</returns>
    Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default);
}