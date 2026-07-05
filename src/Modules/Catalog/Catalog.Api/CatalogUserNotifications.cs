namespace Catalog.Api;

using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Shared.Security;

internal static class CatalogUserNotifications
{
    public static string? GetCurrentUserId(HttpContext httpContext) =>
        httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ??
        httpContext.User.FindFirstValue(ApplicationClaimNames.Subject);
}
