namespace Shared.Administration;

public static class AdminAuditResults
{
    public const int MaxLength = 32;

    public const string Succeeded = "succeeded";
    public const string Denied = "denied";
    public const string Failed = "failed";

    public static string Normalize(string result)
    {
        if (string.IsNullOrWhiteSpace(result))
        {
            throw new ArgumentException("Admin audit result is required.", nameof(result));
        }

        string normalized = result.Trim().ToLowerInvariant();

        return normalized switch
        {
            Succeeded or Denied or Failed => normalized,
            _ => throw new ArgumentException("Admin audit result is not supported.", nameof(result))
        };
    }
}
