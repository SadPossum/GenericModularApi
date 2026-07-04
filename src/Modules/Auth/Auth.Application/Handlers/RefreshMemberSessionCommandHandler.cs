namespace Auth.Application.Handlers;

using Auth.Application.Commands;
using Auth.Contracts;
using Auth.Domain.Aggregates;
using Auth.Domain.Errors;
using Auth.Domain.Repositories;
using Auth.Domain.Services;
using Microsoft.Extensions.Options;
using Shared.Cqrs;
using Shared.Tenancy;
using Shared.Runtime.Time;
using Shared.Results;

internal sealed class RefreshMemberSessionCommandHandler(
    IMemberRepository memberRepository,
    ITokenService tokenService,
    IRefreshTokenHashingService refreshTokenHashingService,
    IOptions<AuthApplicationOptions> options,
    ITenantContext tenantContext,
    ISystemClock clock)
    : ICommandHandler<RefreshMemberSessionCommand, AuthTokensResponse>
{
    public async Task<Result<AuthTokensResponse>> HandleAsync(
        RefreshMemberSessionCommand command,
        CancellationToken cancellationToken)
    {
        AccessTokenClaims? claims = tokenService.GetAccessTokenClaims(command.AccessToken, validateLifetime: false);

        if (claims is null)
        {
            return Result.Failure<AuthTokensResponse>(AuthApplicationErrors.TokenInvalid);
        }

        if (tenantContext.IsEnabled &&
            !string.Equals(tenantContext.TenantId, claims.TenantId, StringComparison.Ordinal))
        {
            return Result.Failure<AuthTokensResponse>(AuthApplicationErrors.TenantMismatch);
        }

        Member? member = await memberRepository.GetByIdAsync(claims.MemberId, cancellationToken).ConfigureAwait(false);

        if (member is null)
        {
            return Result.Failure<AuthTokensResponse>(AuthDomainErrors.MemberNotFound);
        }

        string accessToken = tokenService.GenerateAccessToken(member.Id, member.TenantId, claims.SessionId);
        string refreshToken = tokenService.GenerateRefreshToken();
        string refreshTokenHash = refreshTokenHashingService.HashRefreshToken(command.RefreshToken);
        string newRefreshTokenHash = refreshTokenHashingService.HashRefreshToken(refreshToken);

        Result refreshResult = member.RefreshSession(
            claims.SessionId,
            refreshTokenHash,
            newRefreshTokenHash,
            clock.UtcNow.AddDays(options.Value.RefreshTokenLifetimeDays),
            clock.UtcNow);

        return refreshResult.IsSuccess
            ? Result.Success(new AuthTokensResponse(accessToken, refreshToken))
            : Result.Failure<AuthTokensResponse>(refreshResult.Error);
    }
}
