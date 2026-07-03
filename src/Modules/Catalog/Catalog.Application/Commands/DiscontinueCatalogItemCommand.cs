namespace Catalog.Application.Commands;

using Shared.Application;
using Shared.Application.Cqrs;

public sealed record DiscontinueCatalogItemCommand(Guid ItemId) : ITransactionalCommand<Unit>;
