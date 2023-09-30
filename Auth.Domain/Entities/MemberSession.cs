namespace Auth.Domain.Entities;

using Auth.Domain.Errors;
using Auth.Domain.Exceptions;
using Auth.Domain.Services;
using Auth.Domain.ValueObjects;
using Shared.Domain.Models;
using Shared.ErrorHandling;

public class MemberSession : Entity<MemberSessionId>
{
    public MemberId MemberId { get; }
    public bool IsActive { get; private set; }
    public AccessToken? AccessToken { get; private set; }
    public RefreshToken? RefreshToken { get; private set; }
    public DateTimeOffset LoginDateTime { get; }
    public DateTimeOffset? SignOutDateTime { get; private set; }

    private MemberSession(MemberSessionId id,
        MemberId memberId,
        bool isActive,
        AccessToken? accessToken,
        RefreshToken? refreshToken,
        DateTimeOffset loginDateTime,
        DateTimeOffset? signOutDateTime) : base(id)
    {
        this.MemberId = memberId;
        this.IsActive = isActive;
        this.AccessToken = accessToken;
        this.RefreshToken = refreshToken;
        this.LoginDateTime = loginDateTime;
        this.SignOutDateTime = signOutDateTime;
    }

    internal static MemberSession Create(
        MemberSessionId id,
        MemberId memberId,
        DateTimeOffset loginDateTime,
        ITokenProviderService tokenProvider)
    {
        string accessTokenValue = tokenProvider.GenerateAccessToken(memberId);
        AccessToken accessToken = new(accessTokenValue);

        string refreshTokenValue = tokenProvider.GenerateRefreshToken();
        RefreshToken refreshToken = new(refreshTokenValue, DateTimeOffset.UtcNow);

        MemberSession session = new(id,
            memberId,
            true,
            accessToken,
            refreshToken,
            loginDateTime,
            null);

        return session;
    }

    internal Result ValidateAccessToken(string accessToken, ITokenValidatorService tokenValidator)
    {
        if (!this.IsActive)
        {
            return Result
                .Failure(DomainErrors.MemberSession.SessionDeactivated);
        }

        if (this.AccessToken == null)
        {
            throw new TokenCantBeNullInActiveMemberSessionException();
        }

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return Result
                .Failure(DomainErrors.MemberSession.AccessTokenNotValid);
        }

        if (tokenValidator.IsTokenValid(accessToken, false))
        {
            return Result
                .Failure(DomainErrors.MemberSession.AccessTokenNotValid);
        }

        return Result.Success();
    }

    internal Result<(AccessToken AccessToken, RefreshToken RefreshToken)> RefreshTokens(string accessToken,
        string refreshToken,
        ITokenValidatorService tokenValidator,
        ITokenProviderService tokenProvider,
        int refreshTokenLifeTimeInMinutes)
    {
        Result accessTokenValidationResult = this.ValidateAccessToken(accessToken, tokenValidator);

        if (accessTokenValidationResult.IsFailure)
        {
            return Result.Failure<(AccessToken AccessToken, RefreshToken RefreshToken)>(accessTokenValidationResult.Error);
        }

        Result refreshTokenValidationResult = this.ValidateRefreshToken(refreshToken);

        if (refreshTokenValidationResult.IsFailure)
        {
            return Result.Failure<(AccessToken AccessToken, RefreshToken RefreshToken)>(refreshTokenValidationResult.Error);
        }

        string accessTokenValue = tokenProvider.GenerateAccessToken(this.MemberId);
        this.AccessToken = new(accessTokenValue);

        string refreshTokenValue = tokenProvider.GenerateRefreshToken();
        this.RefreshToken = new(refreshTokenValue, DateTimeOffset.UtcNow.AddMinutes(refreshTokenLifeTimeInMinutes));

        return (this.AccessToken, this.RefreshToken);
    }

    internal Result Deactivate()
    {
        if (!this.IsActive)
        {
            return Result.Failure(DomainErrors.MemberSession.SessionAlreadyDeactivated);
        }

        this.AccessToken = null;
        this.RefreshToken = null;
        this.SignOutDateTime = DateTimeOffset.UtcNow;
        this.IsActive = false;

        return Result.Success();
    }

    private Result ValidateRefreshToken(string refreshToken)
    {
        if (!this.IsActive)
        {
            return Result
                .Failure(DomainErrors.MemberSession.SessionDeactivated);
        }

        if (this.RefreshToken == null)
        {
            throw new TokenCantBeNullInActiveMemberSessionException();
        }

        if (this.RefreshToken.Value != refreshToken)
        {
            return Result
                .Failure(DomainErrors.MemberSession.RefreshTokenNotMatch);
        }

        if (this.RefreshToken.ExpirationDate < DateTime.UtcNow)
        {
            return Result
                .Failure(DomainErrors.MemberSession.RefreshTokenExpired);
        }

        return Result.Success();
    }
}
