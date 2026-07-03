namespace Administration.Persistence.Repositories;

using Administration.Persistence.Entities;
using Shared.Administration;

internal sealed class AdminAuditSink(AdminDbContext dbContext) : IAdminAuditSink
{
    public async Task RecordAsync(AdminAuditRecord record, CancellationToken cancellationToken)
    {
        dbContext.AuditEntries.Add(new AdminAuditEntry(
            record.Id,
            record.ActorId,
            record.TenantId,
            record.Operation,
            record.Permission,
            record.Result,
            record.ErrorCode,
            record.CreatedAtUtc));

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
