using InfraFlowSculptor.Domain.PersonalAccessTokenAggregate;
using InfraFlowSculptor.Domain.PersonalAccessTokenAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Common.Interfaces.Persistence;

/// <summary>
/// Repository interface for <see cref="PersonalAccessToken"/> persistence.
/// </summary>
public interface IPersonalAccessTokenRepository : IRepository<PersonalAccessToken>
{
    /// <summary>Retrieves a token by its SHA-256 hash.</summary>
    /// <param name="tokenHash">The hash to look up.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    Task<PersonalAccessToken?> GetByTokenHashAsync(TokenHash tokenHash, CancellationToken cancellationToken = default);

    /// <summary>Retrieves all non-deleted tokens belonging to a user.</summary>
    /// <param name="userId">The owner's identifier.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    Task<List<PersonalAccessToken>> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken = default);
}
