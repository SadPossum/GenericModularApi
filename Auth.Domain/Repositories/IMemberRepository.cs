namespace Auth.Domain.Repositories;

using Auth.Domain.Aggregates;
using Auth.Domain.ValueObjects;

public interface IMemberRepository
{
    public Member GetMember(MemberId id);

    public Member GetMember(string username);

    public void AddMember(Member member);

    public void DeleteMember(Member member);
}
