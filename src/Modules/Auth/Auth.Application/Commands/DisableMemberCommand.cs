namespace Auth.Application.Commands;

using Shared.Application;
using Shared.Application.Cqrs;

public sealed record DisableMemberCommand(Guid MemberId, string Reason) : ITransactionalCommand<Unit>;
