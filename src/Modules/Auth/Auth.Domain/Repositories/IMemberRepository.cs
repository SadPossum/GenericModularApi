namespace Auth.Domain.Repositories;

using Auth.Domain.Aggregates;
using Auth.Domain.ValueObjects;

public interface IMemberRepository
{
    Task<Member?> GetByIdAsync(MemberId id, CancellationToken cancellationToken);
    Task<Member?> GetByUsernameAsync(string username, CancellationToken cancellationToken);
    Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken);
    Task AddAsync(Member member, CancellationToken cancellationToken);
}
