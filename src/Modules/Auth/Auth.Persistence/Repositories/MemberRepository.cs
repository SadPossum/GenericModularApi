namespace Auth.Persistence.Repositories;

using Auth.Domain.Aggregates;
using Auth.Domain.Entities;
using Auth.Domain.Repositories;
using Auth.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

internal sealed class MemberRepository(AuthDbContext dbContext) : IMemberRepository
{
    public Task<Member?> GetByIdAsync(MemberId id, CancellationToken cancellationToken) =>
        dbContext.Members
            .Include(member => member.Usernames)
            .Include(member => member.Sessions)
            .AsSplitQuery()
            .FirstOrDefaultAsync(member => member.Id == id, cancellationToken);

    public Task<Member?> GetByUsernameAsync(string username, CancellationToken cancellationToken)
    {
        string normalizedUsername = MemberUsername.Normalize(username);

        return dbContext.Members
            .Include(member => member.Usernames)
            .Include(member => member.Sessions)
            .AsSplitQuery()
            .FirstOrDefaultAsync(member => member.Usernames.Any(memberUsername =>
                memberUsername.IsActive && memberUsername.NormalizedValue == normalizedUsername), cancellationToken);
    }

    public Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken)
    {
        string normalizedUsername = MemberUsername.Normalize(username);

        return dbContext.MemberUsernames.AnyAsync(memberUsername =>
            memberUsername.NormalizedValue == normalizedUsername, cancellationToken);
    }

    public async Task AddAsync(Member member, CancellationToken cancellationToken) =>
        await dbContext.Members.AddAsync(member, cancellationToken).ConfigureAwait(false);
}
