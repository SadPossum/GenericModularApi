namespace Catalog.Api;

using Catalog.Application;
using Catalog.Application.Commands;
using Catalog.Application.Queries;
using Catalog.Contracts;
using Catalog.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Hosting;
using Shared.Api.Modules;
using Shared.Api.Observability;
using Shared.Api.Results;
using Shared.Api.Tenancy;
using Shared.Cqrs;
using Shared.Pagination;
using Shared.Results;

public sealed class CatalogModule : IModule
{
    public string Name => CatalogModuleMetadata.Name;

    public void AddServices(IHostApplicationBuilder builder)
    {
        builder.Services.AddCatalogApplication();
        builder.AddCatalogPersistence();
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder items = endpoints.MapGroup("/api/catalog/items")
            .WithModuleName(this.Name)
            .WithTags("Catalog");

        items.MapGet("/", async (
            int? page,
            int? pageSize,
            IRequestDispatcher dispatcher,
            CancellationToken cancellationToken) =>
            (await dispatcher.QueryAsync(
                new ListCatalogItemsQuery(page ?? PageRequest.DefaultPage, pageSize ?? PageRequest.DefaultPageSize),
                cancellationToken).ConfigureAwait(false)).ToHttpResult(PublicErrorStatusCodes))
            .RequireTenant();

        items.MapGet("/{itemId:guid}", async (
            Guid itemId,
            IRequestDispatcher dispatcher,
            CancellationToken cancellationToken) =>
        {
            Result<CatalogItemDto> result = await dispatcher.QueryAsync(
                new GetCatalogItemQuery(itemId),
                cancellationToken).ConfigureAwait(false);

            return result.IsFailure ? result.ToHttpResult(PublicErrorStatusCodes) : Results.Ok(result.Value);
        })
            .RequireTenant();

        items.MapPost("/", async (
            CatalogItemWriteRequest request,
            IRequestDispatcher dispatcher,
            CancellationToken cancellationToken) =>
            (await dispatcher.SendAsync(
                new CreateCatalogItemCommand(request.Sku, request.Name, request.Price, request.Currency),
                cancellationToken).ConfigureAwait(false)).ToHttpResult(PublicErrorStatusCodes))
            .RequireTenant()
            .RequireAuthorization();

        items.MapPut("/{itemId:guid}", async (
            Guid itemId,
            CatalogItemWriteRequest request,
            IRequestDispatcher dispatcher,
            CancellationToken cancellationToken) =>
            (await dispatcher.SendAsync(
                new UpdateCatalogItemCommand(itemId, request.Sku, request.Name, request.Price, request.Currency),
                cancellationToken).ConfigureAwait(false)).ToHttpResult(PublicErrorStatusCodes))
            .RequireTenant()
            .RequireAuthorization();

        items.MapPost("/{itemId:guid}/discontinue", async (
            Guid itemId,
            IRequestDispatcher dispatcher,
            CancellationToken cancellationToken) =>
        {
            Result<Unit> result = await dispatcher.SendAsync(
                new DiscontinueCatalogItemCommand(itemId),
                cancellationToken).ConfigureAwait(false);

            return result.IsSuccess ? Results.NoContent() : result.ToHttpResult(PublicErrorStatusCodes);
        })
            .RequireTenant()
            .RequireAuthorization();
    }

    private static readonly ApiErrorStatusCodeMap PublicErrorStatusCodes = ApiErrorStatusCodeMap.Create(
        new(CatalogApplicationErrors.ItemNotFound.Code, StatusCodes.Status404NotFound),
        new(CatalogApplicationErrors.SkuAlreadyExists.Code, StatusCodes.Status409Conflict),
        new(CatalogApplicationErrors.ItemStatusUnknown.Code, StatusCodes.Status409Conflict),
        new(CatalogApplicationErrors.ItemAlreadyDiscontinued.Code, StatusCodes.Status409Conflict));

    public sealed record CatalogItemWriteRequest(string Sku, string Name, decimal Price, string Currency);
}
