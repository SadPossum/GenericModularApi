namespace Auth.Infrastructure.Services;

using System;
using System.IdentityModel.Tokens.Jwt;
using Auth.Domain.Services;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

internal class JwtValidatorService(IOptions<JwtSettings> settings, RsaSecurityKey rsaSecurityKey) : ITokenValidatorService
{
    public bool IsTokenValid(string token, bool validateLifeTime)
    {
        TokenValidationParameters tokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = validateLifeTime,
            ValidateIssuerSigningKey = true,
            ValidIssuer = settings.Value.Issuer,
            ValidAudience = settings.Value.Audience,
            IssuerSigningKey = rsaSecurityKey,
            ClockSkew = TimeSpan.FromMinutes(0)
        };

        JwtSecurityTokenHandler jwtHandler = new();

        try
        {
            jwtHandler.ValidateToken(token, tokenValidationParameters, out _);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
