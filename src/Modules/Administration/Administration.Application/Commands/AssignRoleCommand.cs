namespace Administration.Application.Commands;

using Shared.Cqrs;

public sealed record AssignRoleCommand(string ActorId, string RoleName, string? TenantId) : ITransactionalCommand<Unit>;
