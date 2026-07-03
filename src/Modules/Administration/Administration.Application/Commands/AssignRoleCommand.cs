namespace Administration.Application.Commands;

using Shared.Application;
using Shared.Application.Cqrs;

public sealed record AssignRoleCommand(string ActorId, string RoleName, string? TenantId) : ITransactionalCommand<Unit>;
