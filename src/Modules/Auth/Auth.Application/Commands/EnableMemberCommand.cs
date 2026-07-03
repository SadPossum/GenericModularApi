namespace Auth.Application.Commands;

using Shared.Application;
using Shared.Application.Cqrs;

public sealed record EnableMemberCommand(Guid MemberId) : ITransactionalCommand<Unit>;
