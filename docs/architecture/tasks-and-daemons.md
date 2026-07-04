# Tasks and Daemons

Tasks and daemons are optional. The shared core defines contracts, module metadata, scheduler-neutral runtime state shapes, and a hosted worker loop. No default host starts a task runner; a host must explicitly compose the task runtime store, worker runtime, and task-owning modules.

## Model

A module owns its task payloads the same way it owns commands, queries, domain events, and persistence.

- A task is a finite run, usually queued or triggered by an operator, API, schedule, or integration event.
- A recurring task is a finite task created repeatedly by a schedule.
- A daemon is long-running work that keeps running until canceled or stopped.

Modules declare owned work with the task metadata builder extension in module metadata:

```csharp
public static ModuleDescriptor Descriptor { get; } = ModuleDescriptor
    .Create(Name)
    .WithTask(
        new ModuleTaskDescriptor(
            "rebuild-search",
            "Rebuild catalog search projection.",
            ModuleTaskKind.OneShot,
            tenantScoped: true,
            supportsControlMessages: true,
            workerGroup: "search-workers",
            payloadVersion: 1))
    .Build();
```

`ModuleTaskDescriptor`, `WithTask(...)`, and `WithTasks(...)` live in `Shared.Tasks`; the generic module descriptor does not own task-specific properties. The descriptor is discoverability and policy metadata. It is not runtime module discovery and does not register a worker.
Task handler identity is `(module, task, payload version)` so modules can keep old payload handlers alive while introducing a new payload shape. Worker group, tenant scope, kind, and control-message support are routing and policy metadata that must still match the module descriptor.

## Payload Contracts

Task payload code implements `ITaskHandler<TPayload>`:

```csharp
internal sealed class RebuildSearchTask : ITaskHandler<RebuildSearchPayload>
{
    public async Task HandleAsync(
        RebuildSearchPayload payload,
        TaskExecutionContext context,
        CancellationToken cancellationToken)
    {
        // use repositories, ports, cache invalidation, or commands through application contracts
    }
}
```

Payloads implement `ITaskPayload`.

`TaskExecutionContext` carries run id, module name, task name, worker group, worker id, node id, attempt, tenant id, correlation id, and whether the run was reclaimed for cancellation. Runtime adapters should pass this context into logging scopes, metrics, audit records, and command dispatch.

Register payload handlers explicitly from the owning module application project:

```csharp
services.AddTaskHandler<GenerateReportTaskPayload, GenerateReportTaskHandler>(
    TaskSamplesModuleMetadata.Name,
    TaskSamplesModuleMetadata.GenerateReportTaskName,
    TaskSamplesModuleMetadata.WorkerGroup,
    tenantScoped: true,
    payloadVersion: 1,
    kind: ModuleTaskKind.OneShot,
    supportsControlMessages: false);
```

Architecture tests compare registered handlers with `ModuleTaskDescriptor` metadata so task docs, module metadata, and runtime registration drift together.

## Communication

Runner-to-system communication uses `ITaskRuntimeReporter`:

- heartbeat reporting;
- bounded progress reporting through `TaskProgress`.

System-to-runner communication uses `ITaskControlChannel`, `ITaskControlLoop`, and `TaskControlMessage`:

- cancellation or pause requests;
- operator commands;
- daemon-specific commands.

`TaskControlCommandNames` defines the standard control commands `tasks.cancel`, `tasks.drain`, `tasks.pause`, and `tasks.resume`. `ITaskControlLoop` is a small helper over the lower-level channel: it polls pending messages, returns a `TaskControlPollResult`, and lets payload code mark messages handled or failed after the handler has actually acted on them. `TaskControlLoopExtensions` adds reusable cooperative behavior for cancel/drain and pause-until-resume loops.

Task payload code that needs to call application behavior should use `ITaskCommandDispatcher` from `Shared.Tasks.Cqrs` or normal CQRS contracts. This keeps payloads independent from HTTP, CLI, scheduler APIs, and module internals.

## Runtime Store Contract

The shared task store contract is intentionally not tied to EF, Quartz.NET, Hangfire, NATS, or hosted services.

`ITaskRunStore` combines enqueueing, lease-based claiming, status reporting, and control messages:

- `TaskRunRequest` represents an enqueue request with module/task identity, worker group, payload JSON, tenant id, correlation id, schedule time, requester, and max attempts.
- `TaskRunRequest.PayloadVersion` selects the matching handler registration.
- `TaskRunRequest.DeduplicationKey` lets producers make active queued/running/retry work idempotent without exposing physical provider keys.
- `TaskWorkerClaim` represents a worker's claim request, including worker group, worker id, node id, batch size, and lease duration.
- `TaskRunLease` is the immutable run lease handed to a worker, including cancellation intent, and can create the `TaskExecutionContext` passed into payload code.
- `TaskRunStatusTransitions` centralizes claim/start/complete/cancel rules so adapters do not invent incompatible state machines.
- `TaskRunStatusNames` centralizes external status names. API, CLI, docs, and metrics use kebab-case names such as `retry-scheduled` and `cancellation-requested`; enum-style names are accepted only as compatibility input.
- `TaskRunStats` and `TaskRunStatsFilter` provide a small operational read model for status counts without coupling operators to EF.

Cancellation is best-effort and lease-aware:

- queued or retry-scheduled runs are marked `Canceled` immediately;
- leased or running runs move to `CancellationRequested`;
- if a cancellation-requested worker disappears, the expired lease can be reclaimed and the replacement worker marks the run `Canceled` without invoking the payload handler.

Heartbeat/progress renews an owned lease when the execution context came from a persisted `TaskRunLease`. This keeps long-running work from being reclaimed solely because it outlived the original claim window. Manually-created execution contexts do not renew leases unless they opt into a positive lease-extension window.

Concrete runtimes persist these concepts in an optional runtime module or adapter. Module application code should not persist task rows directly.

## EF Runtime Module

`TaskRuntime.Persistence` owns the `tasks` schema and maps:

- `task_runs`
- `task_control_messages`

Provider-specific migrations exist for SQL Server and PostgreSQL. The store uses serializable transactions for claim batches and marks runs only when the current worker owns the lease. Runtime store methods are self-committing because workers, scanners, and schedulers usually run outside a CQRS request; the optional `TaskRuntimeUnitOfWork` skips its commit when the store already saved the changes.

Compose it explicitly:

```csharp
builder.AddSharedInfrastructure();
builder.AddTaskInfrastructure();
builder.AddTaskRuntimePersistence();
builder.AddTaskWorkerRuntime();
builder.Services.AddTaskSamplesApplication(); // or your real task-owning modules
```

Worker configuration lives under `Tasks:Worker`:

- `Enabled`
- `WorkerGroups`
- `BatchSize`
- `PollInterval`
- `LeaseDuration`
- `HandlerTimeout`
- `RetryBaseDelay`
- `RetryMaxDelay`
- optional `WorkerId` and `NodeId`
- `MetricsSamplerEnabled`
- `MetricsSamplerPollInterval`

The hosted worker:

- claims due runs by worker group;
- logs transient claim/processing infrastructure failures and retries instead of exiting permanently;
- sets tenant context for tenant-scoped handlers;
- deserializes the registered payload type;
- invokes `ITaskHandler<TPayload>`;
- renews the lease through heartbeat/progress reports;
- marks success only after handler completion;
- marks `TaskRunCanceledException` as terminal `Canceled`;
- marks failure with retry scheduling on handler errors or timeouts;
- marks cancellation-requested reclaimed leases as canceled without requiring the handler to still be registered;
- leaves the lease to expire on host shutdown cancellation.
- processes leases with bounded per-worker-host concurrency;
- emits bounded `{ApplicationIdentity:Namespace}.tasks` metrics for claimed, completed, duration, timed-out, queue-depth, and active-run measurements;
- runs an optional stale timeout scanner that marks abandoned leases/runs as `TimedOut`.

The metrics sampler reads `ITaskRunStore.GetStatsAsync(...)` and updates observable gauges:

- `{ApplicationIdentity:Namespace}.tasks.queue.depth` for `Queued` and `RetryScheduled` runs;
- `{ApplicationIdentity:Namespace}.tasks.active.runs` for `Leased`, `Running`, and `CancellationRequested` runs.

Gauge tags are bounded to task status and do not include tenant ids, payloads, run ids, or control-message ids.

The scheduler, timeout scanner, metrics sampler, and worker loop are long-running infrastructure services. Transient store, provider, or schedule-provider failures are logged and retried on the next poll. Host shutdown cancellation still stops the services cleanly.

## Optional Scheduling

`AddTaskRunScheduling()` starts a small hosted adapter that reads code-defined `ITaskScheduleProvider` schedules and enqueues `TaskRunRequest`s. It is disabled by default through `Tasks:Scheduler:Enabled=false`.

The adapter is intentionally scheduler-neutral:

- schedules are declared in module application code through `ScheduledTaskDefinition`;
- it writes only to `ITaskRunStore`;
- deterministic per-interval dedupe keys are generated as `schedule:<module>:<task>:<schedule>:v<payload-version>:<occurrence>`;
- no domain/application code depends on Quartz.NET, Hangfire, or hosted-service APIs.

External schedulers can still be added later as explicit adapters that create the same `TaskRunRequest` shape.

## Optional Admin

`TaskRuntime.AdminCli` and `TaskRuntime.AdminApi` are optional front doors. They are not registered in `Host.Api`, `Host.AdminCli`, or `Host.AdminApi` by default.

CLI commands:

- `tasks runs list`
- `tasks runs list --status retry-scheduled`
- `tasks runs stats`
- `tasks runs get --run-id <id>`
- `tasks runs enqueue --module <module> --task <task> --payload-json <json>|--payload-file <path>`
- `tasks runs control --run-id <id> --command tasks.pause|tasks.resume|tasks.cancel|tasks.drain --yes`
- `tasks runs cancel --run-id <id> --yes`
- `tasks runs retry --run-id <id> --yes`

Admin API routes live under `/api/admin/tasks/runs` and use the same application commands/queries as the CLI. Permissions are declared as `tasks.runs.read`, `tasks.runs.create`, `tasks.runs.control`, `tasks.runs.cancel`, and `tasks.runs.retry`.

## Remaining Runtime Work

Future optional slices can add:

- live status streaming;
- provider-level stress testing for higher worker counts;
- Quartz.NET or Hangfire adapters if a project needs their clustering, dashboards, calendars, or advanced scheduling semantics.

The default remains small: persistent tasks, hosted workers, code-defined schedules, and admin controls are all opt-in composition.

## Guardrails

- Default hosts do not start task workers.
- Domain projects do not reference scheduler, hosting, EF, HTTP, or admin APIs.
- Application payloads depend on `Shared.Tasks`, `Shared.Tasks.Cqrs` when they dispatch application commands, CQRS contracts, and module ports.
- External scheduler packages stay in explicit adapter projects.
- Store implementations use `ITaskRunStore` and `TaskRunStatusTransitions` instead of ad hoc status changes.
- Task worker hosts call `AddTaskWorkerRuntime()` explicitly and must also compose a concrete `ITaskRunStore`.
- Task scheduler hosts call `AddTaskRunScheduling()` explicitly and must also compose a concrete `ITaskRunStore`.
- Running a task on another node must still use module contracts, integration events, or control messages, not direct cross-module internals.
