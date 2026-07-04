namespace Auth.Application.Commands;

using Shared.Cqrs;

public sealed record DisableMemberCommand(Guid MemberId, string Reason) : ITransactionalCommand<Unit>;
