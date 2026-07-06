namespace Notifications.Api;

using System.Net.ServerSentEvents;
using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Notifications.Application;
using Notifications.Application.Commands;
using Notifications.Application.Queries;
using Notifications.Contracts;
using Notifications.Persistence;
using Shared.Api.Modules;
using Shared.Api.Observability;
using Shared.Api.Results;
using Shared.Api.Tenancy;
using Shared.Cqrs;
using Shared.ModuleComposition;
using Shared.Naming;
using Shared.Results;
using Shared.Security;
using Shared.Tenancy;

public sealed class NotificationsModule : IModule
{
    public string Name => NotificationsModuleMetadata.Name;

    public void AddServices(IHostApplicationBuilder builder)
    {
        builder.SelectModuleProfile(NotificationsProfiles.Default, "Notifications.Api");
        builder.Services.AddNotificationsApplication(builder.Configuration);
        builder.AddNotificationsPersistence();
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/api/notifications")
            .WithModuleName(this.Name)
            .WithTags("Notifications")
            .RequireAuthorization();

        group.MapGet("/", async (
            int? page,
            int? pageSize,
            bool? unreadOnly,
            HttpContext httpContext,
            ITenantContext tenantContext,
            IRequestDispatcher dispatcher,
            CancellationToken cancellationToken) =>
        {
            if (!TryResolveUserContext(httpContext, tenantContext, out string userId, out IResult? failure))
            {
                return failure;
            }

            Result<NotificationHistoryListResponse> result = await dispatcher.QueryAsync(
                new ListNotificationHistoryQuery(
                    userId,
                    unreadOnly ?? false,
                    page ?? Shared.Pagination.PageRequest.DefaultPage,
                    pageSize ?? Shared.Pagination.PageRequest.DefaultPageSize),
                cancellationToken).ConfigureAwait(false);

            return result.ToHttpResult(PublicErrorStatusCodes);
        })
            .RequireTenant();

        group.MapGet("/broadcasts", async (
            int? page,
            int? pageSize,
            bool? unreadOnly,
            HttpContext httpContext,
            ITenantContext tenantContext,
            IRequestDispatcher dispatcher,
            CancellationToken cancellationToken) =>
        {
            if (!TryResolveUserContext(httpContext, tenantContext, out string userId, out IResult? failure))
            {
                return failure;
            }

            Result<NotificationBroadcastListResponse> result = await dispatcher.QueryAsync(
                new ListNotificationBroadcastsQuery(
                    CurrentTenantId(tenantContext),
                    NotificationBroadcastRecipientKind.User,
                    userId,
                    unreadOnly ?? false,
                    page ?? Shared.Pagination.PageRequest.DefaultPage,
                    pageSize ?? Shared.Pagination.PageRequest.DefaultPageSize),
                cancellationToken).ConfigureAwait(false);

            return result.ToHttpResult(PublicErrorStatusCodes);
        })
            .RequireTenant();

        group.MapGet("/broadcasts/{broadcastId:guid}", async (
            Guid broadcastId,
            HttpContext httpContext,
            ITenantContext tenantContext,
            IRequestDispatcher dispatcher,
            CancellationToken cancellationToken) =>
        {
            if (!TryResolveUserContext(httpContext, tenantContext, out string userId, out IResult? failure))
            {
                return failure;
            }

            Result<NotificationBroadcastItem> result = await dispatcher.QueryAsync(
                new GetNotificationBroadcastQuery(
                    broadcastId,
                    CurrentTenantId(tenantContext),
                    NotificationBroadcastRecipientKind.User,
                    userId),
                cancellationToken).ConfigureAwait(false);

            return result.ToHttpResult(PublicErrorStatusCodes);
        })
            .RequireTenant();

        group.MapPost("/broadcasts/{broadcastId:guid}/read", async (
            Guid broadcastId,
            HttpContext httpContext,
            ITenantContext tenantContext,
            IRequestDispatcher dispatcher,
            CancellationToken cancellationToken) =>
        {
            if (!TryResolveUserContext(httpContext, tenantContext, out string userId, out IResult? failure))
            {
                return failure;
            }

            Result<Unit> result = await dispatcher.SendAsync(
                new MarkNotificationBroadcastReadCommand(
                    broadcastId,
                    CurrentTenantId(tenantContext),
                    NotificationBroadcastRecipientKind.User,
                    userId),
                cancellationToken).ConfigureAwait(false);

            return result.IsSuccess ? Results.NoContent() : result.ToHttpResult(PublicErrorStatusCodes);
        })
            .RequireTenant();

        group.MapPost("/broadcasts/read-all", async (
            HttpContext httpContext,
            ITenantContext tenantContext,
            IRequestDispatcher dispatcher,
            CancellationToken cancellationToken) =>
        {
            if (!TryResolveUserContext(httpContext, tenantContext, out string userId, out IResult? failure))
            {
                return failure;
            }

            Result<MarkAllNotificationBroadcastsReadResponse> result = await dispatcher.SendAsync(
                new MarkAllNotificationBroadcastsReadCommand(
                    CurrentTenantId(tenantContext),
                    NotificationBroadcastRecipientKind.User,
                    userId),
                cancellationToken).ConfigureAwait(false);

            return result.ToHttpResult(PublicErrorStatusCodes);
        })
            .RequireTenant();

        group.MapGet("/broadcasts/stream", async (
            long? afterSequence,
            HttpContext httpContext,
            ITenantContext tenantContext,
            IRequestDispatcher dispatcher,
            ILogger<NotificationsModule> logger,
            IOptions<NotificationStreamOptions> streamOptions,
            CancellationToken cancellationToken) =>
        {
            if (!TryResolveUserContext(httpContext, tenantContext, out string userId, out IResult? failure))
            {
                return failure;
            }

            if (afterSequence is < 0)
            {
                return Results.Problem(
                    title: NotificationsApplicationErrors.StreamCursorInvalid.Code,
                    detail: NotificationsApplicationErrors.StreamCursorInvalid.Message,
                    statusCode: StatusCodes.Status400BadRequest);
            }

            string? tenantId = CurrentTenantId(tenantContext);
            long cursor;
            if (afterSequence.HasValue)
            {
                cursor = afterSequence.Value;
            }
            else
            {
                Result<long> cursorResult = await ResolveCurrentBroadcastCursorAsync(
                    dispatcher,
                    tenantId,
                    NotificationBroadcastRecipientKind.User,
                    userId,
                    cancellationToken).ConfigureAwait(false);
                if (cursorResult.IsFailure)
                {
                    return cursorResult.ToHttpResult(PublicErrorStatusCodes);
                }

                cursor = cursorResult.Value;
            }

            IResult stream = TypedResults.ServerSentEvents(
                StreamBroadcastsAsync(
                    dispatcher,
                    tenantId,
                    NotificationBroadcastRecipientKind.User,
                    userId,
                    cursor,
                    streamOptions.Value,
                    logger,
                    httpContext.RequestAborted));
            return stream;
        })
            .RequireTenant();

        group.MapGet("/{notificationId:guid}", async (
            Guid notificationId,
            HttpContext httpContext,
            ITenantContext tenantContext,
            IRequestDispatcher dispatcher,
            CancellationToken cancellationToken) =>
        {
            if (!TryResolveUserContext(httpContext, tenantContext, out string userId, out IResult? failure))
            {
                return failure;
            }

            Result<NotificationHistoryItem> result = await dispatcher.QueryAsync(
                new GetNotificationHistoryItemQuery(notificationId, userId),
                cancellationToken).ConfigureAwait(false);

            return result.ToHttpResult(PublicErrorStatusCodes);
        })
            .RequireTenant();

        group.MapPost("/{notificationId:guid}/read", async (
            Guid notificationId,
            HttpContext httpContext,
            ITenantContext tenantContext,
            IRequestDispatcher dispatcher,
            CancellationToken cancellationToken) =>
        {
            if (!TryResolveUserContext(httpContext, tenantContext, out string userId, out IResult? failure))
            {
                return failure;
            }

            Result<Unit> result = await dispatcher.SendAsync(
                new MarkNotificationReadCommand(notificationId, userId),
                cancellationToken).ConfigureAwait(false);

            return result.IsSuccess ? Results.NoContent() : result.ToHttpResult(PublicErrorStatusCodes);
        })
            .RequireTenant();

        group.MapPost("/read-all", async (
            HttpContext httpContext,
            ITenantContext tenantContext,
            IRequestDispatcher dispatcher,
            CancellationToken cancellationToken) =>
        {
            if (!TryResolveUserContext(httpContext, tenantContext, out string userId, out IResult? failure))
            {
                return failure;
            }

            Result<MarkAllNotificationsReadResponse> result = await dispatcher.SendAsync(
                new MarkAllNotificationsReadCommand(userId),
                cancellationToken).ConfigureAwait(false);

            return result.ToHttpResult(PublicErrorStatusCodes);
        })
            .RequireTenant();

        group.MapGet("/history/stream", async (
            long? afterSequence,
            HttpContext httpContext,
            ITenantContext tenantContext,
            IRequestDispatcher dispatcher,
            ILogger<NotificationsModule> logger,
            IOptions<NotificationStreamOptions> streamOptions,
            CancellationToken cancellationToken) =>
        {
            if (!TryResolveUserContext(httpContext, tenantContext, out string userId, out IResult? failure))
            {
                return failure;
            }

            if (afterSequence is < 0)
            {
                return Results.Problem(
                    title: NotificationsApplicationErrors.StreamCursorInvalid.Code,
                    detail: NotificationsApplicationErrors.StreamCursorInvalid.Message,
                    statusCode: StatusCodes.Status400BadRequest);
            }

            long cursor;
            if (afterSequence.HasValue)
            {
                cursor = afterSequence.Value;
            }
            else
            {
                Result<long> cursorResult = await ResolveCurrentUserCursorAsync(
                    dispatcher,
                    userId,
                    cancellationToken).ConfigureAwait(false);
                if (cursorResult.IsFailure)
                {
                    return cursorResult.ToHttpResult(PublicErrorStatusCodes);
                }

                cursor = cursorResult.Value;
            }

            IResult stream = TypedResults.ServerSentEvents(
                StreamUserHistoryAsync(
                    dispatcher,
                    userId,
                    cursor,
                    streamOptions.Value,
                    logger,
                    httpContext.RequestAborted));
            return stream;
        })
            .RequireTenant();
    }

    private static Task<Result<long>> ResolveCurrentUserCursorAsync(
        IRequestDispatcher dispatcher,
        string userId,
        CancellationToken cancellationToken) =>
        dispatcher.QueryAsync(
            new GetNotificationStreamCursorQuery(userId),
            cancellationToken);

    private static Task<Result<long>> ResolveCurrentBroadcastCursorAsync(
        IRequestDispatcher dispatcher,
        string? tenantId,
        NotificationBroadcastRecipientKind recipientKind,
        string recipientId,
        CancellationToken cancellationToken) =>
        dispatcher.QueryAsync(
            new GetNotificationBroadcastStreamCursorQuery(tenantId, recipientKind, recipientId),
            cancellationToken);

    private static async IAsyncEnumerable<SseItem<NotificationHistoryItem>> StreamUserHistoryAsync(
        IRequestDispatcher dispatcher,
        string userId,
        long initialCursor,
        NotificationStreamOptions options,
        ILogger logger,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        long afterSequence = initialCursor;
        using PeriodicTimer pollTimer = new(options.PollInterval);

        while (!cancellationToken.IsCancellationRequested)
        {
            Result<IReadOnlyList<NotificationHistoryItem>> result = await dispatcher.QueryAsync(
                new StreamNotificationHistoryQuery(userId, afterSequence, options.BatchSize),
                cancellationToken).ConfigureAwait(false);

            if (result.IsFailure)
            {
                LogStreamQueryFailure(logger, "history", result.Error);
                yield break;
            }

            foreach (NotificationHistoryItem item in result.Value)
            {
                afterSequence = item.StreamSequence;
                yield return new SseItem<NotificationHistoryItem>(item, "notification");
            }

            if (!await pollTimer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false))
            {
                yield break;
            }
        }
    }

    private static async IAsyncEnumerable<SseItem<NotificationBroadcastItem>> StreamBroadcastsAsync(
        IRequestDispatcher dispatcher,
        string? tenantId,
        NotificationBroadcastRecipientKind recipientKind,
        string recipientId,
        long initialCursor,
        NotificationStreamOptions options,
        ILogger logger,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        long afterSequence = initialCursor;
        using PeriodicTimer pollTimer = new(options.PollInterval);

        while (!cancellationToken.IsCancellationRequested)
        {
            Result<IReadOnlyList<NotificationBroadcastItem>> result = await dispatcher.QueryAsync(
                new StreamNotificationBroadcastsQuery(
                    tenantId,
                    recipientKind,
                    recipientId,
                    afterSequence,
                    options.BatchSize),
                cancellationToken).ConfigureAwait(false);

            if (result.IsFailure)
            {
                LogStreamQueryFailure(logger, "broadcast", result.Error);
                yield break;
            }

            foreach (NotificationBroadcastItem item in result.Value)
            {
                afterSequence = item.StreamSequence;
                yield return new SseItem<NotificationBroadcastItem>(item, "notification-broadcast");
            }

            if (!await pollTimer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false))
            {
                yield break;
            }
        }
    }

    private static bool TryResolveUserContext(
        HttpContext httpContext,
        ITenantContext tenantContext,
        out string userId,
        out IResult? failure)
    {
        userId = string.Empty;
        failure = null;

        string? candidateUserId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                                  httpContext.User.FindFirstValue(ApplicationClaimNames.Subject);
        if (!NotificationRecipientUserIds.TryNormalize(candidateUserId, out string normalizedUserId))
        {
            failure = Results.Unauthorized();
            return false;
        }

        if (tenantContext.IsEnabled)
        {
            string? tokenTenantId = httpContext.User.FindFirstValue(ApplicationClaimNames.TenantId);
            if (!TenantIds.TryNormalize(tokenTenantId, out string? normalizedTokenTenantId) ||
                !string.Equals(normalizedTokenTenantId, tenantContext.TenantId, StringComparison.Ordinal))
            {
                failure = Results.Forbid();
                return false;
            }
        }

        userId = normalizedUserId;
        return true;
    }

    private static string? CurrentTenantId(ITenantContext tenantContext) =>
        tenantContext.TenantId;

    private static void LogStreamQueryFailure(ILogger logger, string streamName, Error error)
    {
        logger.LogWarning(
            "Notification {StreamName} stream query failed and the stream will be closed. Error: {ErrorCode}.",
            streamName,
            error.Code);
    }

    private static readonly ApiErrorStatusCodeMap PublicErrorStatusCodes = ApiErrorStatusCodeMap.Create(
        new ApiErrorStatusCode(NotificationsApplicationErrors.NotificationNotFound.Code, StatusCodes.Status404NotFound),
        new ApiErrorStatusCode(NotificationsApplicationErrors.BroadcastNotFound.Code, StatusCodes.Status404NotFound));
}
