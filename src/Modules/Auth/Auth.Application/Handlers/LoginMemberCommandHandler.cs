namespace Auth.Application.Handlers;

using Auth.Application.Commands;
using Auth.Contracts;
using Auth.Domain.Aggregates;
using Auth.Domain.Errors;
using Auth.Domain.Repositories;
using Auth.Domain.Services;
using Microsoft.Extensions.Options;
using Shared.Cqrs;
using Shared.Runtime.Identity;
using Shared.Runtime.Time;
using Shared.Results;

internal sealed class LoginMemberCommandHandler(
    IMemberRepository memberRepository,
    IPasswordHashingService passwordHashingService,
    ITokenService tokenService,
    IRefreshTokenHashingService refreshTokenHashingService,
    IOptions<AuthApplicationOptions> options,
    ISystemClock clock,
    IIdGenerator idGenerator)
    : AuthCommandHandlerBase(tokenService, refreshTokenHashingService, clock, idGenerator),
        ICommandHandler<LoginMemberCommand, AuthTokensResponse>
{
    public async Task<Result<AuthTokensResponse>> HandleAsync(
        LoginMemberCommand command,
        CancellationToken cancellationToken)
    {
        Member? member = await memberRepository.GetByUsernameAsync(command.Username, cancellationToken).ConfigureAwait(false);

        if (member is null ||
            !member.HasActiveUsername(command.Username) ||
            !passwordHashingService.VerifyPassword(member.PasswordHash, command.Password))
        {
            return Result.Failure<AuthTokensResponse>(AuthDomainErrors.CredentialsNotValid);
        }

        var tokens = this.CreateTokens(member.Id, member.TenantId, TimeSpan.FromDays(options.Value.RefreshTokenLifetimeDays));
        Result startSessionResult = member.StartSession(tokens.SessionId, tokens.RefreshTokenHash, tokens.ExpiresAtUtc, this.Clock.UtcNow);

        if (startSessionResult.IsFailure)
        {
            return Result.Failure<AuthTokensResponse>(startSessionResult.Error);
        }

        return Result.Success(new AuthTokensResponse(tokens.AccessToken, tokens.RefreshToken));
    }
}
