namespace Shared.Runtime.Infrastructure.Time;

using Shared.Runtime.Time;

internal sealed class SystemClock : ISystemClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
