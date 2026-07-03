namespace Auth.Contracts;

public sealed record AdminMemberDetails(
    Guid MemberId,
    string TenantId,
    string Status,
    string? ActiveUsername,
    DateTimeOffset RegisteredAtUtc,
    DateTimeOffset? DisabledAtUtc,
    string? DisabledReason,
    int ActiveSessionCount,
    int TotalSessionCount);
