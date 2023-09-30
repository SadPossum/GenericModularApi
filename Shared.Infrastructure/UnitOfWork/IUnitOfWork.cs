namespace Shared.Infrastructure.UnitOfWork;

public interface IUnitOfWork
{
    public Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
