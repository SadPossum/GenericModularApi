namespace Shared.Runtime.Infrastructure.Identity;

using Shared.Runtime.Identity;

internal sealed class GuidIdGenerator : IIdGenerator
{
    public Guid NewId() => Guid.NewGuid();
}
