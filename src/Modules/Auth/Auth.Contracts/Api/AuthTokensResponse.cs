namespace Auth.Contracts;

public sealed record AuthTokensResponse(string AccessToken, string RefreshToken);
