namespace Auth.Application.Commands;

using Shared.Cqrs;

public sealed record SignOutAllCommand(Guid MemberId) : ITransactionalCommand<Unit>;
