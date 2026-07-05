namespace Notifications.Admin.Contracts;

using Notifications.Contracts;
using Shared.Administration;

public static class NotificationsAdminPermissions
{
    public static readonly AdminPermission HistoryRead = AdminPermission.Create(NotificationsAdminPermissionCodes.HistoryRead);
    public static readonly AdminPermission BroadcastsRead = AdminPermission.Create(NotificationsAdminPermissionCodes.BroadcastsRead);
    public static readonly AdminPermission BroadcastsCreate = AdminPermission.Create(NotificationsAdminPermissionCodes.BroadcastsCreate);
}
