namespace TaskRuntime.Persistence;

using Shared.Runtime.Time;
using Shared.Tasks.Infrastructure;

internal sealed class TaskRuntimeRunStore(TaskRuntimeDbContext dbContext, ISystemClock clock)
    : EfTaskRunStore<TaskRuntimeDbContext>(dbContext, clock);
