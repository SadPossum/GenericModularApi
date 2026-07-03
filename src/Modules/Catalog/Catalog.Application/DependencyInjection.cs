namespace Catalog.Application;

using Catalog.Application.Commands;
using Catalog.Application.Handlers;
using Catalog.Application.Queries;
using Catalog.Application.Validation;
using Catalog.Contracts;
using Catalog.Domain.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Shared.Application;
using Shared.Application.Cqrs;
using Shared.Application.Events;

public static class DependencyInjection
{
    public static IServiceCollection AddCatalogApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddEnumerable([
            ServiceDescriptor.Scoped<ICommandHandler<CreateCatalogItemCommand, CatalogItemDto>, CreateCatalogItemCommandHandler>(),
            ServiceDescriptor.Scoped<ICommandHandler<UpdateCatalogItemCommand, CatalogItemDto>, UpdateCatalogItemCommandHandler>(),
            ServiceDescriptor.Scoped<ICommandHandler<DiscontinueCatalogItemCommand, Unit>, DiscontinueCatalogItemCommandHandler>(),
            ServiceDescriptor.Scoped<IQueryHandler<GetCatalogItemQuery, CatalogItemDto>, GetCatalogItemQueryHandler>(),
            ServiceDescriptor.Scoped<IQueryHandler<ListCatalogItemsQuery, CatalogItemListResponse>, ListCatalogItemsQueryHandler>(),
            ServiceDescriptor.Scoped<IDomainEventHandler<CatalogItemCreatedDomainEvent>, CatalogItemCreatedOutboxProjector>(),
            ServiceDescriptor.Scoped<IDomainEventHandler<CatalogItemUpdatedDomainEvent>, CatalogItemUpdatedOutboxProjector>(),
            ServiceDescriptor.Scoped<IDomainEventHandler<CatalogItemDiscontinuedDomainEvent>, CatalogItemDiscontinuedOutboxProjector>(),
            ServiceDescriptor.Scoped<ICommandValidator<CreateCatalogItemCommand>, CreateCatalogItemCommandValidator>(),
            ServiceDescriptor.Scoped<ICommandValidator<UpdateCatalogItemCommand>, UpdateCatalogItemCommandValidator>(),
            ServiceDescriptor.Scoped<ICommandValidator<DiscontinueCatalogItemCommand>, DiscontinueCatalogItemCommandValidator>(),
            ServiceDescriptor.Scoped<IQueryValidator<GetCatalogItemQuery>, GetCatalogItemQueryValidator>()
        ]);

        return services;
    }
}
