namespace Shared.Application.Tasks;

public sealed class TaskHandlerRegistration
{
    private TaskHandlerRegistration(
        string moduleName,
        string taskName,
        string workerGroup,
        Type payloadType,
        Type handlerType,
        bool tenantScoped,
        int payloadVersion)
    {
        this.ModuleName = moduleName;
        this.TaskName = taskName;
        this.WorkerGroup = workerGroup;
        this.PayloadType = payloadType;
        this.HandlerType = handlerType;
        this.TenantScoped = tenantScoped;
        this.PayloadVersion = payloadVersion;
    }

    public string ModuleName { get; }
    public string TaskName { get; }
    public string WorkerGroup { get; }
    public Type PayloadType { get; }
    public Type HandlerType { get; }
    public bool TenantScoped { get; }
    public int PayloadVersion { get; }

    public static TaskHandlerRegistration Create<TPayload, THandler>(
        string moduleName,
        string taskName,
        string workerGroup = TaskWorkerGroups.Default,
        bool tenantScoped = true,
        int payloadVersion = 1)
        where TPayload : ITaskPayload
        where THandler : class, ITaskHandler<TPayload>
    {
        return new(
            TaskNames.NormalizeModuleName(moduleName, nameof(moduleName)),
            TaskNames.NormalizeTaskName(taskName, nameof(taskName)),
            TaskNames.NormalizeWorkerGroup(workerGroup, nameof(workerGroup)),
            typeof(TPayload),
            typeof(THandler),
            tenantScoped,
            payloadVersion > 0
                ? payloadVersion
                : throw new ArgumentOutOfRangeException(nameof(payloadVersion), payloadVersion, "Task payload version must be positive."));
    }
}
