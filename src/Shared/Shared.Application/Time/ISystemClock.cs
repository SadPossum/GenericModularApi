namespace Shared.Application.Time;

public interface ISystemClock
{
    DateTimeOffset UtcNow { get; }
}
