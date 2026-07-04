namespace Auth.Application.Commands;

using Shared.Cqrs;

public sealed record SignOutCommand(Guid MemberId, string RefreshToken) : ITransactionalCommand<Unit>;
