namespace Auth.Infrastructure.Services;

using System.Security.Cryptography;
using System.Text;
using Auth.Domain.Services;

public class CryptographyService : ICryptographyService
{
    private const int SaltSize = 64;

    public string GenerateSalt() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(SaltSize));

    public string HashPassword(string password, string salt)
    {
        byte[] saltAsBytes = Encoding.UTF8.GetBytes(salt);
        byte[] passwordAsBytes = Encoding.UTF8.GetBytes(password);

        byte[] saltedValue = [.. passwordAsBytes, .. saltAsBytes];

        return Convert.ToBase64String(SHA256.HashData(saltedValue));
    }
}
