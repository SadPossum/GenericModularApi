namespace Shared.Application.UnitOfWork;

public interface IUnitOfWork
{
    string ModuleName { get; }

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
