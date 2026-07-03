namespace Shared.Application.Tenancy;

public interface ITenantContext
{
    bool IsEnabled { get; }
    string? TenantId { get; }
}
