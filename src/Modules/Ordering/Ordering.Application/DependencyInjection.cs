namespace Ordering.Application;

using Catalog.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ordering.Application.Commands;
using Ordering.Application.Handlers;
using Ordering.Application.Validation;
using Ordering.Contracts;
using Shared.Application.Cqrs;
using Shared.Application.Messaging;

public static class DependencyInjection
{
    public static IServiceCollection AddOrderingApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddEnumerable([
            ServiceDescriptor.Scoped<ICommandHandler<PlaceOrderCommand, OrderDto>, PlaceOrderCommandHandler>(),
            ServiceDescriptor.Scoped<ICommandValidator<PlaceOrderCommand>, PlaceOrderCommandValidator>()
        ]);
        services.AddIntegrationEventHandler<CatalogItemCreatedIntegrationEvent, CatalogItemCreatedProjectionHandler>(
            OrderingModuleMetadata.Name,
            CatalogIntegrationSubjects.ItemCreated,
            OrderingModuleMetadata.CatalogItemCreatedProjectionHandlerName);
        services.AddIntegrationEventHandler<CatalogItemUpdatedIntegrationEvent, CatalogItemUpdatedProjectionHandler>(
            OrderingModuleMetadata.Name,
            CatalogIntegrationSubjects.ItemUpdated,
            OrderingModuleMetadata.CatalogItemUpdatedProjectionHandlerName);
        services.AddIntegrationEventHandler<CatalogItemDiscontinuedIntegrationEvent, CatalogItemDiscontinuedProjectionHandler>(
            OrderingModuleMetadata.Name,
            CatalogIntegrationSubjects.ItemDiscontinued,
            OrderingModuleMetadata.CatalogItemDiscontinuedProjectionHandlerName);

        return services;
    }
}
