namespace Administration.Application.Queries;

using Shared.Application.Cqrs;

public sealed record ListRolesQuery : IQuery<IReadOnlyList<AdminRoleDetails>>;
