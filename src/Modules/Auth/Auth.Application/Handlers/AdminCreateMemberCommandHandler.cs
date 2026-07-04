namespace Auth.Application.Handlers;

using Auth.Application.Commands;
using Auth.Contracts;
using Auth.Domain.Aggregates;
using Auth.Domain.Enums;
using Auth.Domain.Errors;
using Auth.Domain.Repositories;
using Auth.Domain.Services;
using Auth.Domain.ValueObjects;
using Shared.Cqrs;
using Shared.Runtime.Identity;
using Shared.Tenancy;
using Shared.Runtime.Time;
using Shared.Results;

internal sealed class AdminCreateMemberCommandHandler(
    IMemberRepository memberRepository,
    ITenantContext tenantContext,
    IPasswordHashingService passwordHashingService,
    ISystemClock clock,
    IIdGenerator idGenerator)
    : ICommandHandler<AdminCreateMemberCommand, AdminCreatedMemberResponse>
{
    public async Task<Result<AdminCreatedMemberResponse>> HandleAsync(
        AdminCreateMemberCommand command,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(tenantContext.TenantId))
        {
            return Result.Failure<AdminCreatedMemberResponse>(AuthApplicationErrors.TenantRequired);
        }

        Result<MemberUsernameType> usernameType = UsernameTypeMapper.Map(command.UsernameType);
        if (usernameType.IsFailure)
        {
            return Result.Failure<AdminCreatedMemberResponse>(usernameType.Error);
        }

        Result<Member> memberResult = Member.Create(
            new MemberId(idGenerator.NewId()),
            tenantContext.TenantId,
            command.Username,
            usernameType.Value,
            passwordHashingService.HashPassword(command.Password),
            new MemberUsernameId(idGenerator.NewId()),
            idGenerator.NewId(),
            clock.UtcNow);

        if (memberResult.IsFailure)
        {
            return Result.Failure<AdminCreatedMemberResponse>(memberResult.Error);
        }

        if (await memberRepository.UsernameExistsAsync(command.Username, cancellationToken).ConfigureAwait(false))
        {
            return Result.Failure<AdminCreatedMemberResponse>(AuthDomainErrors.UsernameAlreadyExists);
        }

        Member member = memberResult.Value;
        await memberRepository.AddAsync(member, cancellationToken).ConfigureAwait(false);

        return Result.Success(new AdminCreatedMemberResponse(member.Id.Value, command.Username));
    }
}
