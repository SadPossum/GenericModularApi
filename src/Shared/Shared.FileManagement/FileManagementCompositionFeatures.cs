namespace Shared.FileManagement;

using Shared.ModuleComposition;

public static class FileManagementCompositionFeatures
{
    public static readonly CompositionFeatureId Storage = new("file-management.storage");

    public static ProvidedCompositionFeature StorageProvided(string provider) =>
        new(Storage, provider, "File storage backend services are registered.");

    public static RequiredCompositionFeature StorageRequired(string owner, string? reason = null, bool optional = false) =>
        new(Storage, owner, optional, reason);
}
