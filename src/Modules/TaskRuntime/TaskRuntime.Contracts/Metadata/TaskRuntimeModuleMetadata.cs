namespace TaskRuntime.Contracts;

using Shared.Application.Modules;

public static class TaskRuntimeModuleMetadata
{
    public const string Name = "task-runtime";
    public const string Schema = "tasks";
    public const string AdminSurfaceName = "tasks";

    public static ModuleDescriptor Descriptor { get; } = new(
        Name,
        Schema,
        [
            new ModulePermissionDescriptor(TaskRuntimePermissionCodes.RunsRead, "Read task runs.", tenantScoped: false),
            new ModulePermissionDescriptor(TaskRuntimePermissionCodes.RunsCreate, "Create task runs.", tenantScoped: false),
            new ModulePermissionDescriptor(TaskRuntimePermissionCodes.RunsCancel, "Cancel task runs.", tenantScoped: false),
            new ModulePermissionDescriptor(TaskRuntimePermissionCodes.RunsRetry, "Retry task runs.", tenantScoped: false),
            new ModulePermissionDescriptor(TaskRuntimePermissionCodes.RunsControl, "Send task run control messages.", tenantScoped: false),
        ],
        [],
        [],
        [],
        AdminSurfaceName);
}
