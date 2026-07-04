namespace Auth.Application.Handlers;

using Auth.Application.Ports;
using Auth.Application.Queries;
using Auth.Contracts;
using Auth.Domain.Errors;
using Shared.Cqrs;
using Shared.Results;

internal sealed class GetAdminMemberQueryHandler(IAdminMemberReadRepository repository)
    : IQueryHandler<GetAdminMemberQuery, AdminMemberDetails>
{
    public async Task<Result<AdminMemberDetails>> HandleAsync(
        GetAdminMemberQuery query,
        CancellationToken cancellationToken)
    {
        AdminMemberDetails? member = await repository.GetMemberAsync(query.MemberId, cancellationToken).ConfigureAwait(false);

        return member is null
            ? Result.Failure<AdminMemberDetails>(AuthDomainErrors.MemberNotFound)
            : Result.Success(member);
    }
}
