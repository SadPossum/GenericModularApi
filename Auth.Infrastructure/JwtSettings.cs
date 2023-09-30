namespace Auth.Infrastructure;

public class JwtSettings(string issuer, string audience, string publicKey, string privateKey, long accessTokenLifeTimeInSeconds, int refreshTokenLength, int refreshTokenLifeTimeInMinutes)
{
    public string Issuer { get; set; } = issuer;
    public string Audience { get; set; } = audience;
    public string PublicKey { get; set; } = publicKey;
    public string PrivateKey { get; set; } = privateKey;
    public long AccessTokenLifeTimeInSeconds { get; set; } = accessTokenLifeTimeInSeconds;
    public int RefreshTokenLength { get; set; } = refreshTokenLength;
    public int RefreshTokenLifeTimeInMinutes { get; set; } = refreshTokenLifeTimeInMinutes;
}

