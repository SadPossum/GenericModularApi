namespace Shared.Infrastructure.Time;

using Shared.Application.Time;

internal sealed class SystemClock : ISystemClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
