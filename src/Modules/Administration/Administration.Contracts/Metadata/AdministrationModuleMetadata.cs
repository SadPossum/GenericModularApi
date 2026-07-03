namespace Administration.Contracts;

using Shared.Application.Modules;

public static class AdministrationModuleMetadata
{
    public const string Name = "administration";
    public const string Schema = "admin";
    public const string AdminSurfaceName = "admin";

    public static ModuleDescriptor Descriptor { get; } = new(
        Name,
        Schema,
        [
            new ModulePermissionDescriptor(AdministrationPermissionCodes.Bootstrap, "Bootstrap the first administration owner.", tenantScoped: false),
            new ModulePermissionDescriptor(AdministrationPermissionCodes.RolesRead, "Read administration roles.", tenantScoped: false),
            new ModulePermissionDescriptor(AdministrationPermissionCodes.RolesManage, "Manage administration roles and assignments.", tenantScoped: false),
        ],
        [],
        [],
        [],
        AdminSurfaceName);
}

