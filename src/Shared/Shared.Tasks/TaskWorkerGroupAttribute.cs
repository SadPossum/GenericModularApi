namespace Shared.Tasks;

using Shared.Modules;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
public sealed class TaskWorkerGroupAttribute(string workerGroup) : Attribute
{
    public string WorkerGroup { get; } = TaskNames.NormalizeWorkerGroup(workerGroup, nameof(workerGroup));

    public static string GetOrDefault(Type payloadType)
    {
        ArgumentNullException.ThrowIfNull(payloadType);

        return ModuleMetadataAttributeReader
            .GetOptional<TaskWorkerGroupAttribute>(payloadType)
            ?.WorkerGroup ?? TaskWorkerGroups.Default;
    }
}
