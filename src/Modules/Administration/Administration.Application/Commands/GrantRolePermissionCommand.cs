namespace Administration.Application.Commands;

using Shared.Cqrs;

public sealed record GrantRolePermissionCommand(string RoleName, string PermissionCode) : ITransactionalCommand<Unit>;
