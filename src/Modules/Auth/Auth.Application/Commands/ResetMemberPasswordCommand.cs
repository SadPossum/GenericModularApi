namespace Auth.Application.Commands;

using Shared.Cqrs;

public sealed record ResetMemberPasswordCommand(Guid MemberId, string NewPassword) : ITransactionalCommand<Unit>;
