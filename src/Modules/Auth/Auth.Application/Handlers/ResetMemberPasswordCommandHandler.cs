namespace Auth.Application.Handlers;

using Auth.Application.Commands;
using Auth.Domain.Aggregates;
using Auth.Domain.Errors;
using Auth.Domain.Repositories;
using Auth.Domain.Services;
using Auth.Domain.ValueObjects;
using Shared.Cqrs;
using Shared.Results;

internal sealed class ResetMemberPasswordCommandHandler(
    IMemberRepository memberRepository,
    IPasswordHashingService passwordHashingService)
    : ICommandHandler<ResetMemberPasswordCommand, Unit>
{
    public async Task<Result<Unit>> HandleAsync(ResetMemberPasswordCommand command, CancellationToken cancellationToken)
    {
        Member? member = await memberRepository.GetByIdAsync(new MemberId(command.MemberId), cancellationToken).ConfigureAwait(false);

        if (member is null)
        {
            return Result.Failure<Unit>(AuthDomainErrors.MemberNotFound);
        }

        Result result = member.ResetPassword(passwordHashingService.HashPassword(command.NewPassword));

        return result.IsSuccess ? Result.Success(Unit.Value) : Result.Failure<Unit>(result.Error);
    }
}
