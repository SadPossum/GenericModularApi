namespace Auth.Domain.Services;

using Auth.Domain.ValueObjects;
using Shared.Domain;

public interface ITokenService
{
    string GenerateAccessToken(MemberId memberId, string tenantId, MemberSessionId sessionId);
    string GenerateRefreshToken();
    MemberId? GetMemberId(string accessToken, bool validateLifetime);
    AccessTokenClaims? GetAccessTokenClaims(string accessToken, bool validateLifetime);
}

public sealed record AccessTokenClaims
{
    public AccessTokenClaims(MemberId memberId, string tenantId, MemberSessionId sessionId)
    {
        if (memberId.Value == Guid.Empty)
        {
            throw new ArgumentException("Member id is required.", nameof(memberId));
        }

        if (sessionId.Value == Guid.Empty)
        {
            throw new ArgumentException("Member session id is required.", nameof(sessionId));
        }

        this.MemberId = memberId;
        this.TenantId = TenantIds.Normalize(tenantId);
        this.SessionId = sessionId;
    }

    public MemberId MemberId { get; }
    public string TenantId { get; }
    public MemberSessionId SessionId { get; }
}
