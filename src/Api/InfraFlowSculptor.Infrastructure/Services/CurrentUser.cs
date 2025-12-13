using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using Microsoft.AspNetCore.Http;

namespace InfraFlowSculptor.Infrastructure.Services;

public class CurrentUser:  ICurrentUser
{
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly IUserRepository userRepository;

    public CurrentUser(
        IHttpContextAccessor httpContextAccessor,
        IUserRepository userRepository)
    {
        this.httpContextAccessor = httpContextAccessor;
        this.userRepository = userRepository;
    }

    public async Task<UserId> GetUserIdAsync(CancellationToken cancellationToken = default)
    {
        var principal = httpContextAccessor.HttpContext?.User
                        ?? throw new UnauthorizedAccessException();

        var entraId = principal.FindFirst("oid")?.Value
                      ?? throw new UnauthorizedAccessException("Missing oid claim");
        
        var nameClaim = principal.Identity?.Name
                        ?? throw new UnauthorizedAccessException("Missing name claim");
        
        //TODO more checks
        var firstName = nameClaim.Split(' ')[0];
        var lastName = nameClaim.Split(' ')[1];

        var user = await userRepository.GetOrCreateByEntraIdAsync(entraId, firstName, lastName, cancellationToken);

        return user.Id;
    }
}