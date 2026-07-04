namespace Shared.Tasks;

public sealed class TaskHandlerRegistration
{
    private TaskHandlerRegistration(
        string moduleName,
        string taskName,
        string workerGroup,
        Type payloadType,
        Type handlerType,
        ModuleTaskKind kind,
        bool tenantScoped,
        int payloadVersion,
        bool supportsControlMessages)
    {
        this.ModuleName = moduleName;
        this.TaskName = taskName;
        this.WorkerGroup = workerGroup;
        this.PayloadType = payloadType;
        this.HandlerType = handlerType;
        this.Kind = kind;
        this.TenantScoped = tenantScoped;
        this.PayloadVersion = payloadVersion;
        this.SupportsControlMessages = supportsControlMessages;
    }

    public string ModuleName { get; }
    public string TaskName { get; }
    public string WorkerGroup { get; }
    public Type PayloadType { get; }
    public Type HandlerType { get; }
    public ModuleTaskKind Kind { get; }
    public bool TenantScoped { get; }
    public int PayloadVersion { get; }
    public bool SupportsControlMessages { get; }

    public static TaskHandlerRegistration Create<TPayload, THandler>(
        string moduleName,
        string taskName,
        string workerGroup = TaskWorkerGroups.Default,
        bool tenantScoped = true,
        int payloadVersion = 1,
        ModuleTaskKind kind = ModuleTaskKind.OneShot,
        bool supportsControlMessages = false)
        where TPayload : ITaskPayload
        where THandler : class, ITaskHandler<TPayload>
    {
        return new(
            TaskNames.NormalizeModuleName(moduleName, nameof(moduleName)),
            TaskNames.NormalizeTaskName(taskName, nameof(taskName)),
            TaskNames.NormalizeWorkerGroup(workerGroup, nameof(workerGroup)),
            typeof(TPayload),
            typeof(THandler),
            kind is ModuleTaskKind.Unknown || !Enum.IsDefined(kind)
                ? throw new ArgumentException("Task kind must be a known non-unknown value.", nameof(kind))
                : kind,
            tenantScoped,
            payloadVersion > 0
                ? payloadVersion
                : throw new ArgumentOutOfRangeException(nameof(payloadVersion), payloadVersion, "Task payload version must be positive."),
            supportsControlMessages);
    }
}
