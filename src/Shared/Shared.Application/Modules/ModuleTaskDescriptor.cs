namespace Shared.Application.Modules;

using Shared.Application.Tasks;

public sealed record ModuleTaskDescriptor
{
    public ModuleTaskDescriptor(
        string name,
        string description,
        ModuleTaskKind kind,
        bool tenantScoped,
        bool supportsControlMessages,
        string workerGroup = TaskWorkerGroups.Default,
        int payloadVersion = 1)
    {
        this.Name = TaskNames.NormalizeTaskName(name, nameof(name));
        this.Description = NormalizeDescription(description);
        this.Kind = kind is ModuleTaskKind.Unknown || !Enum.IsDefined(kind)
            ? throw new ArgumentException("Task kind must be a known non-unknown value.", nameof(kind))
            : kind;
        this.TenantScoped = tenantScoped;
        this.SupportsControlMessages = supportsControlMessages;
        this.WorkerGroup = TaskNames.NormalizeWorkerGroup(workerGroup, nameof(workerGroup));
        this.PayloadVersion = payloadVersion > 0
            ? payloadVersion
            : throw new ArgumentOutOfRangeException(nameof(payloadVersion), payloadVersion, "Task payload version must be positive.");
    }

    public string Name { get; }
    public string Description { get; }
    public ModuleTaskKind Kind { get; }
    public bool TenantScoped { get; }
    public bool SupportsControlMessages { get; }
    public string WorkerGroup { get; }
    public int PayloadVersion { get; }

    private static string NormalizeDescription(string description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        string normalized = description.Trim();
        if (normalized.Any(char.IsControl))
        {
            throw new ArgumentException("Task description cannot contain control characters.", nameof(description));
        }

        return normalized;
    }
}
