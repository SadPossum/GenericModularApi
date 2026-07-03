namespace Auth.Contracts;

public sealed record AdminMemberListItem(
    Guid MemberId,
    string TenantId,
    string Status,
    string? ActiveUsername,
    DateTimeOffset RegisteredAtUtc,
    int ActiveSessionCount);
