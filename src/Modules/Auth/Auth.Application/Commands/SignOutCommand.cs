namespace Auth.Application.Commands;

using Shared.Application;
using Shared.Application.Cqrs;

public sealed record SignOutCommand(Guid MemberId, string RefreshToken) : ITransactionalCommand<Unit>;
