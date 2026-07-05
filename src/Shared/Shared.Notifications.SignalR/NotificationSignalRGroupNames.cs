namespace Shared.Notifications.SignalR;

using System.Security.Cryptography;
using System.Text;
using Shared.Naming;

internal static class NotificationSignalRGroupNames
{
    public static string ForUser(string applicationNamespace, string tenantId, string userId)
    {
        string normalizedNamespace = ApplicationNamespaces.Normalize(applicationNamespace);
        string input = $"{tenantId}\u001f{userId}";
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));

        return $"{normalizedNamespace}:notifications:user:{Convert.ToHexString(hash).ToLowerInvariant()}";
    }
}
