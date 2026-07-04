namespace Auth.Api;

using System.Security.Claims;
using Auth.Application;
using Auth.Application.Commands;
using Auth.Contracts;
using Auth.Infrastructure;
using Auth.Infrastructure.JwtBearer;
using Auth.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Hosting;
using Shared.Api.Modules;
using Shared.Api.Observability;
using Shared.Api.Results;
using Shared.Api.Tenancy;
using Shared.Cqrs;
using Shared.Security;
using Shared.Tenancy;
using Shared.Results;

public sealed class AuthModule : IModule
{
    public string Name => AuthModuleMetadata.Name;

    public void AddServices(IHostApplicationBuilder builder)
    {
        builder.Services.AddAuthApplication(builder.Configuration);
        builder.Services.AddAuthInfrastructure(builder.Configuration);
        builder
            .AddAuthJwtBearerAuthentication()
            .AddAuthPersistence();
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/api/auth")
            .WithModuleName(this.Name)
            .WithTags("Auth");

        group.MapPost("/register", async (
            RegisterMemberRequest request,
            IRequestDispatcher dispatcher,
            CancellationToken cancellationToken) =>
            (await dispatcher.SendAsync(
                new RegisterMemberCommand(request.Username, request.UsernameType, request.Password),
                cancellationToken).ConfigureAwait(false)).ToHttpResult(PublicErrorStatusCodes))
            .RequireTenant();

        group.MapPost("/login", async (
            LoginMemberRequest request,
            IRequestDispatcher dispatcher,
            CancellationToken cancellationToken) =>
            (await dispatcher.SendAsync(
                new LoginMemberCommand(request.Username, request.Password),
                cancellationToken).ConfigureAwait(false)).ToHttpResult(PublicErrorStatusCodes))
            .RequireTenant();

        group.MapPost("/refresh", async (
            RefreshTokenRequest request,
            IRequestDispatcher dispatcher,
            CancellationToken cancellationToken) =>
            (await dispatcher.SendAsync(
                new RefreshMemberSessionCommand(request.AccessToken, request.RefreshToken),
                cancellationToken).ConfigureAwait(false)).ToHttpResult(PublicErrorStatusCodes))
            .RequireTenant();

        group.MapPost("/sign-out", async (
            SignOutRequest request,
            ClaimsPrincipal user,
            ITenantContext tenantContext,
            IRequestDispatcher dispatcher,
            CancellationToken cancellationToken) =>
        {
            if (!TokenTenantMatches(user, tenantContext))
            {
                return Results.Unauthorized();
            }

            Guid? memberId = GetMemberId(user);

            if (memberId is null)
            {
                return Results.Unauthorized();
            }

            Result<Unit> result = await dispatcher.SendAsync(
                new SignOutCommand(memberId.Value, request.RefreshToken),
                cancellationToken).ConfigureAwait(false);

            return result.IsSuccess ? Results.NoContent() : result.ToHttpResult(PublicErrorStatusCodes);
        })
            .RequireTenant()
            .RequireAuthorization();

        group.MapPost("/sign-out-all", async (
            ClaimsPrincipal user,
            ITenantContext tenantContext,
            IRequestDispatcher dispatcher,
            CancellationToken cancellationToken) =>
        {
            if (!TokenTenantMatches(user, tenantContext))
            {
                return Results.Unauthorized();
            }

            Guid? memberId = GetMemberId(user);

            if (memberId is null)
            {
                return Results.Unauthorized();
            }

            Result<Unit> result = await dispatcher.SendAsync(
                new SignOutAllCommand(memberId.Value),
                cancellationToken).ConfigureAwait(false);

            return result.IsSuccess ? Results.NoContent() : result.ToHttpResult(PublicErrorStatusCodes);
        })
            .RequireTenant()
            .RequireAuthorization();
    }

    private static readonly ApiErrorStatusCodeMap PublicErrorStatusCodes = ApiErrorStatusCodeMap.Create(
        new(AuthApplicationErrors.CredentialsNotValid.Code, StatusCodes.Status401Unauthorized),
        new(AuthApplicationErrors.TokenInvalid.Code, StatusCodes.Status401Unauthorized),
        new(AuthApplicationErrors.MemberNotFound.Code, StatusCodes.Status401Unauthorized),
        new(AuthApplicationErrors.SessionNotFound.Code, StatusCodes.Status401Unauthorized),
        new(AuthApplicationErrors.SessionInactive.Code, StatusCodes.Status401Unauthorized),
        new(AuthApplicationErrors.RefreshTokenInvalid.Code, StatusCodes.Status401Unauthorized),
        new(AuthApplicationErrors.RefreshTokenExpired.Code, StatusCodes.Status401Unauthorized),
        new(AuthApplicationErrors.TenantMismatch.Code, StatusCodes.Status403Forbidden),
        new(AuthApplicationErrors.MemberStatusUnknown.Code, StatusCodes.Status403Forbidden),
        new(AuthApplicationErrors.MemberDisabled.Code, StatusCodes.Status403Forbidden),
        new(AuthApplicationErrors.UsernameAlreadyExists.Code, StatusCodes.Status409Conflict));

    private static Guid? GetMemberId(ClaimsPrincipal user)
    {
        string? memberId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(memberId, out Guid parsed)
            ? parsed
            : null;
    }

    private static bool TokenTenantMatches(ClaimsPrincipal user, ITenantContext tenantContext)
    {
        if (!tenantContext.IsEnabled)
        {
            return true;
        }

        string? tokenTenantId = user.FindFirstValue(GmaClaimNames.TenantId);

        return !string.IsNullOrWhiteSpace(tokenTenantId) &&
               string.Equals(tokenTenantId, tenantContext.TenantId, StringComparison.Ordinal);
    }
}
