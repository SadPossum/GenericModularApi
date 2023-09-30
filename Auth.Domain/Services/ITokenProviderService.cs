namespace Auth.Domain.Services;

using Auth.Domain.ValueObjects;

public interface ITokenProviderService
{
    string GenerateAccessToken(MemberId memberId);

    string GenerateRefreshToken();
}
