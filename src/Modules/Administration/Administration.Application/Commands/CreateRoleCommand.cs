namespace Administration.Application.Commands;

using Shared.Application.Cqrs;

public sealed record CreateRoleCommand(string Name) : ITransactionalCommand<AdminRoleDetails>;
