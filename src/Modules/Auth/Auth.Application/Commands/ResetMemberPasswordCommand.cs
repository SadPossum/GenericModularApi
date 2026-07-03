namespace Auth.Application.Commands;

using Shared.Application;
using Shared.Application.Cqrs;

public sealed record ResetMemberPasswordCommand(Guid MemberId, string NewPassword) : ITransactionalCommand<Unit>;
