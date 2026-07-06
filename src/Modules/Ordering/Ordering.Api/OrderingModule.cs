namespace Ordering.Api;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Hosting;
using Ordering.Application;
using Ordering.Application.Commands;
using Ordering.Application.Queries;
using Ordering.Contracts;
using Ordering.Persistence;
using Shared.Api.Modules;
using Shared.Api.Observability;
using Shared.Api.Results;
using Shared.Api.Tenancy;
using Shared.Cqrs;
using Shared.ModuleComposition;
using Shared.Pagination;
using Shared.Results;

public sealed class OrderingModule : IModule
{
    public string Name => OrderingModuleMetadata.Name;

    public void AddServices(IHostApplicationBuilder builder)
    {
        builder.SelectModuleProfile(OrderingProfiles.Default, "Ordering.Api");
        builder.Services.AddOrderingApplication();
        builder.AddOrderingPersistence();
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder orders = endpoints.MapGroup("/api/orders")
            .WithModuleName(this.Name)
            .WithTags("Ordering");

        orders.MapGet("/", async (
            int? page,
            int? pageSize,
            IRequestDispatcher dispatcher,
            CancellationToken cancellationToken) =>
            (await dispatcher.QueryAsync(
                new ListOrdersQuery(page ?? PageRequest.DefaultPage, pageSize ?? PageRequest.DefaultPageSize),
                cancellationToken).ConfigureAwait(false)).ToHttpResult(PublicErrorStatusCodes))
            .RequireTenant()
            .RequireAuthorization();

        orders.MapGet("/{orderId:guid}", async (
            Guid orderId,
            IRequestDispatcher dispatcher,
            CancellationToken cancellationToken) =>
        {
            Result<OrderDto> result = await dispatcher.QueryAsync(
                new GetOrderQuery(orderId),
                cancellationToken).ConfigureAwait(false);

            return result.ToHttpResult(PublicErrorStatusCodes);
        })
            .RequireTenant()
            .RequireAuthorization();

        orders.MapPost("/", async (
            PlaceOrderRequest request,
            IRequestDispatcher dispatcher,
            CancellationToken cancellationToken) =>
            (await dispatcher.SendAsync(
                new PlaceOrderCommand(request.CatalogItemId, request.Quantity),
                cancellationToken).ConfigureAwait(false)).ToHttpResult(PublicErrorStatusCodes))
            .RequireTenant()
            .RequireAuthorization();
    }

    public sealed record PlaceOrderRequest(Guid CatalogItemId, int Quantity);

    private static readonly ApiErrorStatusCodeMap PublicErrorStatusCodes = ApiErrorStatusCodeMap.Create(
        new(OrderingApplicationErrors.OrderNotFound.Code, StatusCodes.Status404NotFound),
        new(OrderingApplicationErrors.CatalogItemUnknown.Code, StatusCodes.Status409Conflict),
        new(OrderingApplicationErrors.CatalogItemDiscontinued.Code, StatusCodes.Status409Conflict),
        new(OrderingApplicationErrors.CatalogItemStatusUnknown.Code, StatusCodes.Status409Conflict));
}
