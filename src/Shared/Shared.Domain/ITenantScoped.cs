namespace Shared.Domain;

public interface ITenantScoped
{
    string TenantId { get; }
}
