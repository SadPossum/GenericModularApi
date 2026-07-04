namespace Administration.Application.Commands;

using Shared.Cqrs;

public sealed record BootstrapOwnerCommand(string ActorId, bool Confirmed) : ITransactionalCommand<Unit>;
