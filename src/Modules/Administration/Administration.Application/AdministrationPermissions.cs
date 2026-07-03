namespace Administration.Application;

using Administration.Contracts;
using Shared.Administration;

public static class AdministrationPermissions
{
    public static readonly AdminPermission Bootstrap = AdminPermission.Create(AdministrationPermissionCodes.Bootstrap);
    public static readonly AdminPermission RolesRead = AdminPermission.Create(AdministrationPermissionCodes.RolesRead);
    public static readonly AdminPermission RolesManage = AdminPermission.Create(AdministrationPermissionCodes.RolesManage);
}
