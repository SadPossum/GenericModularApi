namespace Auth.Application.Commands;

using Shared.Cqrs;

public sealed record EnableMemberCommand(Guid MemberId) : ITransactionalCommand<Unit>;
