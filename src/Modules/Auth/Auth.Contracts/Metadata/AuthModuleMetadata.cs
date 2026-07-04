namespace Auth.Contracts;

using Shared.Authorization;
using Shared.Messaging;
using Shared.Modules;

public static class AuthModuleMetadata
{
    public const string Name = "auth";
    public const string Schema = "auth";

    public static ModuleDescriptor Descriptor { get; } = ModuleDescriptor
        .Create(Name)
        .WithSchema(Schema)
        .WithPermissions([
            new ModulePermissionDescriptor(AuthAdminPermissionCodes.MembersRead, "Read Auth members.", tenantScoped: true),
            new ModulePermissionDescriptor(AuthAdminPermissionCodes.MembersCreate, "Create Auth members.", tenantScoped: true),
            new ModulePermissionDescriptor(AuthAdminPermissionCodes.MembersDisable, "Disable Auth members.", tenantScoped: true),
            new ModulePermissionDescriptor(AuthAdminPermissionCodes.MembersEnable, "Enable Auth members.", tenantScoped: true),
            new ModulePermissionDescriptor(AuthAdminPermissionCodes.MembersResetPassword, "Reset Auth member passwords.", tenantScoped: true),
            new ModulePermissionDescriptor(AuthAdminPermissionCodes.MembersRevokeSessions, "Revoke Auth member sessions.", tenantScoped: true),
        ])
        .WithPublishedEvent<MemberRegisteredIntegrationEvent>()
        .WithPublishedEvent<MemberDisabledIntegrationEvent>()
        .WithPublishedEvent<MemberEnabledIntegrationEvent>()
        .WithPublishedEvent<MemberSessionsRevokedIntegrationEvent>()
        .Build();
}
