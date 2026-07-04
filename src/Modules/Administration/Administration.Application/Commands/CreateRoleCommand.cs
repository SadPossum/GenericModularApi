namespace Administration.Application.Commands;

using Shared.Cqrs;

public sealed record CreateRoleCommand(string Name) : ITransactionalCommand<AdminRoleDetails>;
