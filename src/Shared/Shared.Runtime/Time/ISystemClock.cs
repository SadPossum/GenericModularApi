namespace Shared.Runtime.Time;

public interface ISystemClock
{
    DateTimeOffset UtcNow { get; }
}
