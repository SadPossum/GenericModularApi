namespace Administration.Application.Commands;

using Shared.Application;
using Shared.Application.Cqrs;

public sealed record GrantRolePermissionCommand(string RoleName, string PermissionCode) : ITransactionalCommand<Unit>;
