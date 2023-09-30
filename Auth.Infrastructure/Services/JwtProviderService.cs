namespace Auth.Infrastructure.Services;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Auth.Domain.Services;
using Auth.Domain.ValueObjects;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

internal class JwtProviderService(IOptions<JwtSettings> settings, RsaSecurityKey rsaSecurityKey) : ITokenProviderService
{
    public string GenerateAccessToken(MemberId memberId)
    {
        SigningCredentials signingCredentials = new(
                key: rsaSecurityKey,
                algorithm: SecurityAlgorithms.RsaSha256
            );

        ClaimsIdentity claimsIdentity = new();

        claimsIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, memberId.Value.ToString()));

        JwtSecurityTokenHandler jwtHandler = new();

        JwtSecurityToken jwt = jwtHandler.CreateJwtSecurityToken(
            issuer: settings.Value.Issuer,
            audience: settings.Value.Audience,
            subject: claimsIdentity,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddSeconds(settings.Value.AccessTokenLifeTimeInSeconds),
            issuedAt: DateTime.UtcNow,
            signingCredentials: signingCredentials);

        string serializedJwt = jwtHandler.WriteToken(jwt);

        return serializedJwt;
    }

    public string GenerateRefreshToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(settings.Value.RefreshTokenLength));
}
