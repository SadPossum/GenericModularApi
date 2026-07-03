namespace Auth.Application;

using Shared.ErrorHandling;

public static class AuthApplicationErrors
{
    public static readonly Error TenantRequired = new("Auth.TenantRequired", "A tenant id is required.");
    public static readonly Error UsernameTypeInvalid = new("Auth.UsernameTypeInvalid", "Username type must be email or phone.");
    public static readonly Error TokenInvalid = new("Auth.TokenInvalid", "Access token is invalid.");
    public static readonly Error TenantMismatch = new("Auth.TenantMismatch", "Access token tenant does not match the active tenant.");
}
