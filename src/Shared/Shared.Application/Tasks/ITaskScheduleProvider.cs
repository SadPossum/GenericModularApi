namespace Shared.Application.Tasks;

public interface ITaskScheduleProvider
{
    Task<IReadOnlyList<ScheduledTaskDefinition>> GetSchedulesAsync(CancellationToken cancellationToken);
}
