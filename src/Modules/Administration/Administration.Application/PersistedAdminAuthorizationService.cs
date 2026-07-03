namespace Administration.Application;

using Administration.Application.Ports;
using Shared.Administration;

internal sealed class PersistedAdminAuthorizationService(IAdminRbacRepository repository) : IAdminAuthorizationService
{
    public async Task<AdminAuthorizationResult> AuthorizeAsync(
        AdminActor actor,
        AdminPermission permission,
        string? tenantId,
        CancellationToken cancellationToken)
    {
        bool allowed = await repository.HasPermissionAsync(actor.Id, permission.Code, tenantId, cancellationToken)
            .ConfigureAwait(false);

        return allowed
            ? AdminAuthorizationResult.Allowed()
            : AdminAuthorizationResult.Denied(AdminErrors.Unauthorized.Message);
    }
}
