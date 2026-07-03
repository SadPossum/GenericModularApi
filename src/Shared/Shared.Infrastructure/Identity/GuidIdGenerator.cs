namespace Shared.Infrastructure.Identity;

using Shared.Application.Identity;

internal sealed class GuidIdGenerator : IIdGenerator
{
    public Guid NewId() => Guid.NewGuid();
}
