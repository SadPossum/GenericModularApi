namespace Auth.Contracts;

using Shared.Messaging;

public static class AuthIntegrationSubjects
{
    public const string MemberRegisteredEventName = "member-registered";
    public const string MemberDisabledEventName = "member-disabled";
    public const string MemberEnabledEventName = "member-enabled";
    public const string MemberSessionsRevokedEventName = "member-sessions-revoked";
    public const int CurrentVersion = 1;

    public static string MemberRegistered => CreateMemberRegistered();
    public static string MemberDisabled => CreateMemberDisabled();
    public static string MemberEnabled => CreateMemberEnabled();
    public static string MemberSessionsRevoked => CreateMemberSessionsRevoked();

    public static string CreateMemberRegistered(string subjectPrefix = IntegrationEventNaming.DefaultSubjectPrefix) =>
        IntegrationEventNaming.CreateSubject(subjectPrefix, AuthModuleMetadata.Name, MemberRegisteredEventName, CurrentVersion);

    public static string CreateMemberDisabled(string subjectPrefix = IntegrationEventNaming.DefaultSubjectPrefix) =>
        IntegrationEventNaming.CreateSubject(subjectPrefix, AuthModuleMetadata.Name, MemberDisabledEventName, CurrentVersion);

    public static string CreateMemberEnabled(string subjectPrefix = IntegrationEventNaming.DefaultSubjectPrefix) =>
        IntegrationEventNaming.CreateSubject(subjectPrefix, AuthModuleMetadata.Name, MemberEnabledEventName, CurrentVersion);

    public static string CreateMemberSessionsRevoked(string subjectPrefix = IntegrationEventNaming.DefaultSubjectPrefix) =>
        IntegrationEventNaming.CreateSubject(subjectPrefix, AuthModuleMetadata.Name, MemberSessionsRevokedEventName, CurrentVersion);
}
