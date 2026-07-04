namespace Auth.Application.Handlers;

using Auth.Application.Commands;
using Auth.Domain.Aggregates;
using Auth.Domain.Errors;
using Auth.Domain.Repositories;
using Auth.Domain.ValueObjects;
using Shared.Cqrs;
using Shared.Runtime.Time;
using Shared.Results;

internal sealed class SignOutAllCommandHandler(IMemberRepository memberRepository, ISystemClock clock)
    : ICommandHandler<SignOutAllCommand, Unit>
{
    public async Task<Result<Unit>> HandleAsync(SignOutAllCommand command, CancellationToken cancellationToken)
    {
        Member? member = await memberRepository.GetByIdAsync(new MemberId(command.MemberId), cancellationToken)
            .ConfigureAwait(false);

        if (member is null)
        {
            return Result.Failure<Unit>(AuthDomainErrors.MemberNotFound);
        }

        Result result = member.SignOutAll(clock.UtcNow);

        return result.IsSuccess
            ? Result.Success(Unit.Value)
            : Result.Failure<Unit>(result.Error);
    }
}
