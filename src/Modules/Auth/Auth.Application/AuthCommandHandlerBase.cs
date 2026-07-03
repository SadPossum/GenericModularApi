namespace Auth.Application;

using Auth.Domain.Services;
using Auth.Domain.ValueObjects;
using Shared.Application.Identity;
using Shared.Application.Time;

internal abstract class AuthCommandHandlerBase(
    ITokenService tokenService,
    IRefreshTokenHashingService refreshTokenHashingService,
    ISystemClock clock,
    IIdGenerator idGenerator)
{
    protected ISystemClock Clock => clock;
    protected IIdGenerator IdGenerator => idGenerator;

    protected (MemberSessionId SessionId, string AccessToken, string RefreshToken, string RefreshTokenHash, DateTimeOffset ExpiresAtUtc)
        CreateTokens(MemberId memberId, string tenantId, TimeSpan refreshTokenLifetime)
    {
        MemberSessionId sessionId = new(this.IdGenerator.NewId());
        string accessToken = tokenService.GenerateAccessToken(memberId, tenantId, sessionId);
        string refreshToken = tokenService.GenerateRefreshToken();
        string refreshTokenHash = refreshTokenHashingService.HashRefreshToken(refreshToken);
        DateTimeOffset expiresAtUtc = this.Clock.UtcNow.Add(refreshTokenLifetime);

        return (sessionId, accessToken, refreshToken, refreshTokenHash, expiresAtUtc);
    }
}
