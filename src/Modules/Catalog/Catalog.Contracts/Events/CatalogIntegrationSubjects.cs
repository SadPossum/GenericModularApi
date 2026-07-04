namespace Catalog.Contracts;

using Shared.Messaging;

public static class CatalogIntegrationSubjects
{
    public const string ItemCreatedEventName = "item-created";
    public const string ItemUpdatedEventName = "item-updated";
    public const string ItemDiscontinuedEventName = "item-discontinued";
    public const int CurrentVersion = 1;

    public static string ItemCreated => CreateItemCreated();
    public static string ItemUpdated => CreateItemUpdated();
    public static string ItemDiscontinued => CreateItemDiscontinued();

    public static string CreateItemCreated(string subjectPrefix = IntegrationEventNaming.DefaultSubjectPrefix) =>
        IntegrationEventNaming.CreateSubject(subjectPrefix, CatalogModuleMetadata.Name, ItemCreatedEventName, CurrentVersion);

    public static string CreateItemUpdated(string subjectPrefix = IntegrationEventNaming.DefaultSubjectPrefix) =>
        IntegrationEventNaming.CreateSubject(subjectPrefix, CatalogModuleMetadata.Name, ItemUpdatedEventName, CurrentVersion);

    public static string CreateItemDiscontinued(string subjectPrefix = IntegrationEventNaming.DefaultSubjectPrefix) =>
        IntegrationEventNaming.CreateSubject(subjectPrefix, CatalogModuleMetadata.Name, ItemDiscontinuedEventName, CurrentVersion);
}
