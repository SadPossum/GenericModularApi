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
        .WithPublishedEvents([
            new ModuleIntegrationEventDescriptor("member-registered", AuthIntegrationSubjects.MemberRegistered, 1, tenantScoped: true),
            new ModuleIntegrationEventDescriptor("member-disabled", AuthIntegrationSubjects.MemberDisabled, 1, tenantScoped: true),
            new ModuleIntegrationEventDescriptor("member-enabled", AuthIntegrationSubjects.MemberEnabled, 1, tenantScoped: true),
            new ModuleIntegrationEventDescriptor("member-sessions-revoked", AuthIntegrationSubjects.MemberSessionsRevoked, 1, tenantScoped: true),
        ])
        .Build();
}
