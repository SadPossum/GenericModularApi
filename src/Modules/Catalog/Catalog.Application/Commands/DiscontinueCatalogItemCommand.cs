namespace Catalog.Application.Commands;

using Shared.Cqrs;

public sealed record DiscontinueCatalogItemCommand(Guid ItemId) : ITransactionalCommand<Unit>;
