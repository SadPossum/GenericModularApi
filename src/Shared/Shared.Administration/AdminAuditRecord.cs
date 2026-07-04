namespace Shared.Administration;

using Shared.Naming;
using Shared.Results;

public sealed record AdminAuditRecord
{
    public const int ErrorCodeMaxLength = Error.CodeMaxLength;

    public AdminAuditRecord(
        Guid id,
        string actorId,
        string? tenantId,
        string operation,
        string permission,
        string result,
        string? errorCode,
        DateTimeOffset createdAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Admin audit id is required.", nameof(id));
        }

        AdminPermission normalizedPermission = AdminPermission.Create(permission);

        this.Id = id;
        this.ActorId = AdminActor.System(actorId).Id;
        this.TenantId = string.IsNullOrWhiteSpace(tenantId)
            ? null
            : TenantIds.Normalize(tenantId);
        this.Operation = AdminOperation.Create(operation, normalizedPermission).Name;
        this.Permission = normalizedPermission.Code;
        this.Result = AdminAuditResults.Normalize(result);
        this.ErrorCode = NormalizeErrorCode(errorCode);
        this.CreatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; }
    public string ActorId { get; }
    public string? TenantId { get; }
    public string Operation { get; }
    public string Permission { get; }
    public string Result { get; }
    public string? ErrorCode { get; }
    public DateTimeOffset CreatedAtUtc { get; }

    private static string? NormalizeErrorCode(string? errorCode)
    {
        if (string.IsNullOrWhiteSpace(errorCode))
        {
            return null;
        }

        if (!Error.TryNormalizeCode(errorCode, out string? normalized))
        {
            throw new ArgumentException(
                $"Admin audit error code must be a valid error code, {ErrorCodeMaxLength} characters or fewer.",
                nameof(errorCode));
        }

        return normalized;
    }
}
