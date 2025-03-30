using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using InfraFlowSculptor.Application.Common.Interfaces.Authentication;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Domain.UserAggregate;

namespace InfraFlowSculptor.Infrastructure.Authentication;

public class JwtGenerator(IOptions<JwtSettings> jwtSettings, IDateTimeProvider dateTimeProvider)
    : IJwtGenerator
{
    private readonly JwtSettings _jwtSettings = jwtSettings.Value;
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

    public string GenerateToken(User user)
    {
        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret)),
            SecurityAlgorithms.HmacSha256Signature);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.Value.ToString()!),
            new Claim(JwtRegisteredClaimNames.GivenName, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("role", user.Role),
            new Claim("firstName", user.Name.FirstName),
            new Claim("lastName", user.Name.LastName),
        };

        var securityToken = new JwtSecurityToken(
            audience: _jwtSettings.Audience,
            issuer: _jwtSettings.Issuer,
            expires: _dateTimeProvider.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
            claims: claims,
            signingCredentials: signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(securityToken);
    }
}