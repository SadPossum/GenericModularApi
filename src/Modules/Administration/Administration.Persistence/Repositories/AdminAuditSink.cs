namespace Administration.Persistence.Repositories;

using Administration.Persistence.Entities;
using Shared.Administration;

internal sealed class AdminAuditSink(AdminDbContext dbContext) : IAdminAuditSink
{
    public async Task RecordAsync(AdminAuditRecord record, CancellationToken cancellationToken)
    {
        dbContext.AuditEntries.Add(new AdminAuditEntry(record));

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
