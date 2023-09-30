namespace Auth.Domain.Aggregates;

using System.Collections.Generic;
using Auth.Domain.Entities;
using Auth.Domain.Enums;
using Auth.Domain.Errors;
using Auth.Domain.Services;
using Auth.Domain.ValueObjects;
using Shared.Domain.Models;
using Shared.ErrorHandling;

public class Member : AggregateRoot<MemberId>
{
    private readonly HashSet<MemberUsername> _usernames = new();
    private readonly string _passwordSalt;
    private readonly List<MemberSession> _sessions = new();
    private string _passwordHash;

    public IReadOnlySet<MemberUsername> Usernames => this._usernames;
    public IReadOnlyList<MemberSession> Sessions => this._sessions;

    private Member(MemberId id,
        string passwordSalt,
        string passwordHash) : base(id)
    {
        this._passwordSalt = passwordSalt;
        this._passwordHash = passwordHash;
    }

    public static Result<Member> Create(MemberId id,
            IEnumerable<(string Value, MemberUsernameType UsernameType)> usernames,
            string password,
            ICryptographyService cryptography)
    {
        string salt = cryptography.GenerateSalt();
        string passwordHash = cryptography.HashPassword(password, salt);

        Member member = new(id, salt, passwordHash);

        foreach ((string value, MemberUsernameType usernameType) in usernames)
        {
            Result<MemberUsername> usernameResult = member.AddUsername(value, usernameType);

            if (usernameResult.IsFailure)
            {
                return Result.Failure<Member>(usernameResult.Error);
            }
        }

        return member;
    }

    public Result<MemberSession> Login(string username,
        string password,
        ICryptographyService cryptography,
        ITokenProviderService tokenProvider)
    {
        if (!this.IsValidCredentials(username, password, cryptography))
        {
            return Result.Failure<MemberSession>(DomainErrors.Member.CredentialsNotValid);
        }

        MemberSession? session = MemberSession.Create(new(Guid.NewGuid()),
            this.Id,
            DateTimeOffset.UtcNow,
            tokenProvider);

        this._sessions.Add(session);

        return session;
    }

    public Result ValidateAccessToken(string accessToken, ITokenValidatorService tokenValidator)
    {
        MemberSession? session = this._sessions.Find(a => a.AccessToken?.Value == accessToken);

        if (session == null)
        {
            return Result
                .Failure<(AccessToken AccessToken, RefreshToken RefreshToken)>(DomainErrors.Member.SessionNotFound);
        }

        return session.ValidateAccessToken(accessToken, tokenValidator);
    }

    public Result<(AccessToken AccessToken, RefreshToken RefreshToken)> RefreshSessionTokens(string accessToken,
        string refreshToken,
        ITokenValidatorService tokenValidator,
        ITokenProviderService tokenProvider,
        int refreshTokenLifeTimeInMinutes)
    {
        MemberSession? session = this._sessions.Find(a => a.AccessToken?.Value == accessToken);

        if (session == null)
        {
            return Result
                .Failure<(AccessToken AccessToken, RefreshToken RefreshToken)>(DomainErrors.Member.SessionNotFound);
        }

        return session.RefreshTokens(accessToken, refreshToken, tokenValidator, tokenProvider, refreshTokenLifeTimeInMinutes);
    }

    public Result SignOut(MemberSessionId sessionId)
    {
        MemberSession? session = this._sessions.Find(a => a.Id == sessionId);

        if (session == null)
        {
            return Result.Failure(DomainErrors.Member.SessionNotFound);
        }

        return session.Deactivate();
    }

    public Result SignOutAllSessions()
    {
        IEnumerable<MemberSession> activeSessions = this._sessions.FindAll(a => a.IsActive);

        if (!activeSessions.Any())
        {
            return Result.Failure(DomainErrors.MemberSession.NoAnyActiveSessionHaveBeenFound);
        }

        foreach (MemberSession session in activeSessions)
        {
            session.Deactivate();
        }

        return Result.Success();
    }

    public Result SignOutAllSessionsExceptOne(MemberSessionId sessionId)
    {
        if (!this._sessions.Exists(a => a.Id == sessionId))
        {
            return Result.Failure(DomainErrors.Member.SessionNotFound);
        }

        IEnumerable<MemberSession> activeSessions = this._sessions.FindAll(a => a.IsActive && a.Id != sessionId);

        if (!activeSessions.Any())
        {
            return Result.Failure(DomainErrors.MemberSession.NoAnyActiveSessionHaveBeenFound);
        }

        foreach (MemberSession session in activeSessions)
        {
            session.Deactivate();
        }

        return Result.Success();
    }

    public Result<MemberUsername> AddUsername(string value, MemberUsernameType usernameType)
    {
        // Member can have only one active username per type (email, phone, etc.) 
        // Deactivating previous username with the same type
        MemberUsername? oldUsername = this._usernames.FirstOrDefault(a => a.UsernameType == usernameType);

        if (oldUsername is not null && oldUsername.IsActive)
        {
            oldUsername.Deactivate();
        }

        Result<MemberUsername> usernameResult = MemberUsername.Create(new(Guid.NewGuid()),
            this.Id,
            value,
            usernameType);

        if (usernameResult.IsSuccess)
        {
            this._usernames.Add(usernameResult.Value);
        }

        return usernameResult;
    }

    public Result ChangePassword(string oldPassword,
        string newPassword,
        ICryptographyService cryptography)
    {
        if (string.IsNullOrWhiteSpace(oldPassword) || string.IsNullOrWhiteSpace(newPassword))
        {
            return Result.Failure(DomainErrors.Member.PasswordNotValid);
        }

        string oldPasswordHash = cryptography.HashPassword(oldPassword, this._passwordSalt);

        if (this._passwordHash != oldPasswordHash)
        {
            return Result.Failure(DomainErrors.Member.OldPasswordNotMatch);
        }

        if (!this.IsStrongPassword(newPassword))
        {
            return Result.Failure(DomainErrors.Member.PasswordNotValid);
        }

        this._passwordHash = cryptography.HashPassword(newPassword, this._passwordSalt);

        return Result.Success();
    }

    private bool IsValidCredentials(string username,
        string password,
        ICryptographyService cryptography)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return false;
        }

        string passwordHash = cryptography.HashPassword(password, this._passwordSalt);

        bool isLoginVerified = this._usernames.Any(a => a.Value == username && a.IsActive);
        bool isPasswordVerified = this._passwordHash == passwordHash;

        return isLoginVerified && isPasswordVerified;
    }

    private bool IsStrongPassword(string password) =>
        // Password strength criteria:
        // 1. Minimum length of 8 characters.
        // 2. At least one uppercase letter.
        // 3. At least one lowercase letter.
        // 4. At least one digit.
        // 5. At least one special character (e.g., !@#$%^&*).
        !string.IsNullOrWhiteSpace(password) &&
            password.Length >= 8 &&
            password.Any(char.IsUpper) &&
            password.Any(char.IsLower) &&
            password.Any(char.IsDigit) &&
            password.Any(this.IsSpecialCharacter);

    private bool IsSpecialCharacter(char character)
    {
        string specialCharacters = "!@#$%^&*";

        return specialCharacters.Contains(character, StringComparison.Ordinal);
    }
}
