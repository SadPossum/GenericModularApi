namespace Auth.Persistence;

using System.Threading.Tasks;
using Auth.Application;

internal class AuthUnitOfWork(AuthDbContext authDbContext) : IAuthUnitOfWork
{
    private readonly AuthDbContext _authDbContext = authDbContext;

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        this._authDbContext.SaveChangesAsync(cancellationToken);
}
