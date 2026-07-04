namespace Administration.Application.Queries;

using Shared.Cqrs;

public sealed record ListRolesQuery : IQuery<IReadOnlyList<AdminRoleDetails>>;
