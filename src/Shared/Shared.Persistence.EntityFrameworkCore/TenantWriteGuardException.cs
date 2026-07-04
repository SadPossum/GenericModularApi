namespace Shared.Persistence.EntityFrameworkCore;

public sealed class TenantWriteGuardException(string message) : InvalidOperationException(message)
{
}
