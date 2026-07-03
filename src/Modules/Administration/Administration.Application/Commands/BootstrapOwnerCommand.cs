namespace Administration.Application.Commands;

using Shared.Application;
using Shared.Application.Cqrs;

public sealed record BootstrapOwnerCommand(string ActorId, bool Confirmed) : ITransactionalCommand<Unit>;
