namespace Auth.Application.Commands;

using Shared.Application;
using Shared.Application.Cqrs;

public sealed record SignOutAllCommand(Guid MemberId) : ITransactionalCommand<Unit>;
