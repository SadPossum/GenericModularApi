namespace Auth.Domain.Services;

public interface ITokenValidatorService
{
    public bool IsTokenValid(string token, bool validateLifeTime);
}
