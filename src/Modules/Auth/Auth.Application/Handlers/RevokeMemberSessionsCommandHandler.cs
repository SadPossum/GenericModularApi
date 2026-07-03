namespace Auth.Application.Handlers;

using Auth.Application.Commands;
using Auth.Contracts;
using Auth.Domain.Aggregates;
using Auth.Domain.Errors;
using Auth.Domain.Repositories;
using Auth.Domain.ValueObjects;
using Shared.Application.Cqrs;
using Shared.Application.Identity;
using Shared.Application.Time;
using Shared.ErrorHandling;

internal sealed class RevokeMemberSessionsCommandHandler(
    IMemberRepository memberRepository,
    ISystemClock clock,
    IIdGenerator idGenerator)
    : ICommandHandler<RevokeMemberSessionsCommand, AdminRevokeSessionsResponse>
{
    public async Task<Result<AdminRevokeSessionsResponse>> HandleAsync(
        RevokeMemberSessionsCommand command,
        CancellationToken cancellationToken)
    {
        Member? member = await memberRepository.GetByIdAsync(new MemberId(command.MemberId), cancellationToken).ConfigureAwait(false);

        if (member is null)
        {
            return Result.Failure<AdminRevokeSessionsResponse>(AuthDomainErrors.MemberNotFound);
        }

        Result<int> result = member.RevokeSessions(idGenerator.NewId(), clock.UtcNow);

        return result.IsSuccess
            ? Result.Success(new AdminRevokeSessionsResponse(result.Value))
            : Result.Failure<AdminRevokeSessionsResponse>(result.Error);
    }
}
