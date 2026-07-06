namespace Shared.Tests;

using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shared.FileManagement;
using Shared.FileManagement.LocalStorage;
using Xunit;

[Trait("Category", "Unit")]
public sealed class FileManagementTests
{
    [Fact]
    public void Object_keys_reject_path_traversal_and_whitespace()
    {
        Assert.True(FileStorageObjectKey.TryCreate("files/tenant-123/avatar", out FileStorageObjectKey? safeKey));
        Assert.Equal("files/tenant-123/avatar", safeKey.Value.Value);

        Assert.False(FileStorageObjectKey.TryCreate("files/../secret", out _));
        Assert.False(FileStorageObjectKey.TryCreate("/files/avatar", out _));
        Assert.False(FileStorageObjectKey.TryCreate("files//avatar", out _));
        Assert.False(FileStorageObjectKey.TryCreate("files/avatar name", out _));
    }

    [Fact]
    public async Task Local_storage_round_trips_content_and_metadata()
    {
        string root = Path.Combine(Path.GetTempPath(), $"gma-files-{Guid.NewGuid():N}");

        try
        {
            using ServiceProvider provider = BuildLocalProvider(root);
            IFileStorage storage = provider.GetRequiredService<IFileStorage>();
            FileStorageObjectKey key = new("files/global/avatar");
            byte[] payload = Encoding.UTF8.GetBytes("hello-files");

            await using MemoryStream input = new(payload);
            FileStorageObjectProperties stored = await storage.PutAsync(new FileStorageWriteRequest(
                key,
                input,
                payload.Length,
                "text/plain",
                "avatar.txt",
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["module"] = "files"
                }));

            Assert.Equal(payload.Length, stored.ContentLength);
            Assert.Equal("text/plain", stored.ContentType);
            Assert.Equal("avatar.txt", stored.FileName);

            FileStorageReadResult? read = await storage.OpenReadAsync(key);
            Assert.NotNull(read);

            await using MemoryStream output = new();
            await read.CopyToAsync(output);

            Assert.Equal(payload, output.ToArray());
            Assert.True(await storage.DeleteAsync(key));
            Assert.Null(await storage.GetPropertiesAsync(key));
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }

    private static ServiceProvider BuildLocalProvider(string root)
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder();
        builder.Configuration["FileManagement:Enabled"] = "true";
        builder.Configuration["FileManagement:Provider"] = "LocalStorage";
        builder.Configuration["FileManagement:MaximumObjectBytes"] = "1048576";
        builder.Configuration["FileManagement:LocalStorage:RootPath"] = root;
        builder.AddLocalFileStorage();
        return builder.Services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
    }
}
