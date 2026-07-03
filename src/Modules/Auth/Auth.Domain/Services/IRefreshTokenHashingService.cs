namespace Auth.Domain.Services;

public interface IRefreshTokenHashingService
{
    string HashRefreshToken(string refreshToken);
}
