namespace Auth.Persistence.Repositories;

using Auth.Domain.Aggregates;
using Auth.Domain.Repositories;
using Auth.Domain.ValueObjects;

internal class MemberRepository : IMemberRepository
{
    public Member GetMember(MemberId id) => throw new NotImplementedException();

    public Member GetMember(string username) => throw new NotImplementedException();

    public void AddMember(Member member) => throw new NotImplementedException();

    public void DeleteMember(Member member) => throw new NotImplementedException();
}
