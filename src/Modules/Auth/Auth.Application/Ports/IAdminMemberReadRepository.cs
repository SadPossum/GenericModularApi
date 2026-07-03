namespace Auth.Application.Ports;

using Auth.Contracts;
using Shared.Application.Queries;

public interface IAdminMemberReadRepository
{
    Task<AdminMemberListResponse> ListMembersAsync(PageRequest pageRequest, CancellationToken cancellationToken);
    Task<AdminMemberDetails?> GetMemberAsync(Guid memberId, CancellationToken cancellationToken);
}
