namespace TaskRuntime.Persistence;

using Shared.Application.Time;
using Shared.Infrastructure.Tasks;

internal sealed class TaskRuntimeRunStore(TaskRuntimeDbContext dbContext, ISystemClock clock)
    : EfTaskRunStore<TaskRuntimeDbContext>(dbContext, clock);
