namespace Files.Application;

using System.Security.Cryptography;
using System.Text;
using Shared.FileManagement;
using Shared.Tenancy;

internal static class FilesStorageKeys
{
    public static FileStorageObjectKey For(Guid fileId, ITenantContext tenantContext)
    {
        if (fileId == Guid.Empty)
        {
            throw new ArgumentException("File id cannot be empty.", nameof(fileId));
        }

        string tenantSegment = TenantSegment(tenantContext);
        return new FileStorageObjectKey($"files/{tenantSegment}/{fileId:N}");
    }

    private static string TenantSegment(ITenantContext tenantContext)
    {
        if (!tenantContext.IsEnabled)
        {
            return "global";
        }

        if (string.IsNullOrWhiteSpace(tenantContext.TenantId))
        {
            throw new InvalidOperationException("Tenant id is required when tenancy is enabled.");
        }

        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(tenantContext.TenantId));
        string hashText = Convert.ToHexString(hash).ToLowerInvariant();
        return $"tenant-{hashText[..16]}";
    }
}
