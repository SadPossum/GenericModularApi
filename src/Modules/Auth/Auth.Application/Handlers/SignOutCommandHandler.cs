namespace Auth.Application.Handlers;

using Auth.Application.Commands;
using Auth.Domain.Aggregates;
using Auth.Domain.Errors;
using Auth.Domain.Repositories;
using Auth.Domain.Services;
using Auth.Domain.ValueObjects;
using Shared.Application;
using Shared.Application.Cqrs;
using Shared.Application.Time;
using Shared.ErrorHandling;

internal sealed class SignOutCommandHandler(
    IMemberRepository memberRepository,
    IRefreshTokenHashingService refreshTokenHashingService,
    ISystemClock clock)
    : ICommandHandler<SignOutCommand, Unit>
{
    public async Task<Result<Unit>> HandleAsync(SignOutCommand command, CancellationToken cancellationToken)
    {
        Member? member = await memberRepository.GetByIdAsync(new MemberId(command.MemberId), cancellationToken)
            .ConfigureAwait(false);

        if (member is null)
        {
            return Result.Failure<Unit>(AuthDomainErrors.MemberNotFound);
        }

        Result result = member.SignOut(
            refreshTokenHashingService.HashRefreshToken(command.RefreshToken),
            clock.UtcNow);

        return result.IsSuccess
            ? Result.Success(Unit.Value)
            : Result.Failure<Unit>(result.Error);
    }
}
