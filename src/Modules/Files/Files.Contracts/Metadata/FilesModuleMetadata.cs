namespace Files.Contracts;

using Shared.ModuleComposition;
using Shared.Modules;

public static class FilesModuleMetadata
{
    public const string Name = "files";

    public static ModuleDescriptor Descriptor { get; } = ModuleDescriptor
        .Create(Name)
        .WithProfile(FilesProfiles.Default)
        .Build();
}
