using InfraFlowSculptor.Domain.UserAggregate;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Common.Interfaces.Persistence;

/// <summary>Repository abstraction for the <see cref="User"/> aggregate.</summary>
public interface IUserRepository : IRepository<User>
{
    /// <summary>
    /// Retrieves a user by their Microsoft Entra ID.
    /// </summary>
    /// <param name="entraId">The Entra object identifier.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The matching <see cref="User"/>, or <c>null</c> if not found.</returns>
    Task<User?> GetByEntraIdAsync(string entraId, CancellationToken cancellationToken = default);

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