namespace Files.Contracts;

using Shared.FileManagement;
using Shared.ModuleComposition;
using Shared.Tenancy;

public static class FilesProfiles
{
    public const string DefaultName = "default";

    public static ModuleProfileDescriptor Default { get; } = new(
        FilesModuleMetadata.Name,
        DefaultName,
        provides:
        [
            FilesCompositionFeatures.ObjectsProvided(Provider(DefaultName))
        ],
        requires:
        [
            new RequiredCompositionFeature(
                TenancyCompositionFeatures.Context,
                Provider(DefaultName),
                reason: "Files are tenant-partitioned when tenancy is enabled; register TenancyModule or Shared.Tenancy.Infrastructure."),
            new RequiredCompositionFeature(
                new CompositionFeatureId(FileManagementCompositionFeatures.Storage),
                Provider(DefaultName),
                reason: "Files stores bytes through Shared.FileManagement; register a concrete adapter such as LocalStorage or MinIO.")
        ],
        displayName: "Files default",
        description: "Tenant-scoped file upload, download, and delete front door backed by shared file storage.");

    private static string Provider(string profileName) => $"{FilesModuleMetadata.Name}/{profileName}";
}
