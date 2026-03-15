using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.Identity.Web;

namespace InfraFlowSculptor.Infrastructure.Services;

public class CurrentUser:  ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserRepository _userRepository;

    public CurrentUser(
        IHttpContextAccessor httpContextAccessor,
        IUserRepository userRepository)
    {
        _httpContextAccessor = httpContextAccessor;
        _userRepository = userRepository;
    }

    public async Task<UserId> GetUserIdAsync(CancellationToken cancellationToken = default)
    {
        var principal = _httpContextAccessor.HttpContext?.User
                        ?? throw new UnauthorizedAccessException();

        var entraId = principal.FindFirst(ClaimConstants.ObjectId)?.Value
                      ?? throw new UnauthorizedAccessException("Missing oid claim");
        
        var nameClaim = principal.FindFirst(ClaimConstants.Name)?.Value
                        ?? throw new UnauthorizedAccessException("Missing name claim");
        
        var firstName = string.Empty;
        var lastName = nameClaim;
        
        if (nameClaim.Contains(' '))
        {
            firstName = nameClaim.Split(' ')[0];
            lastName = nameClaim.Split(' ')[1];
        }

        var user = await _userRepository.GetOrCreateByEntraIdAsync(entraId, firstName, lastName, cancellationToken);

        return user.Id;
    }
}