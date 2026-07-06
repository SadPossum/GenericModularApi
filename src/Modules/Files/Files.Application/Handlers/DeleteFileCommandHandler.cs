namespace Files.Application.Handlers;

using Files.Application.Commands;
using Shared.Cqrs;
using Shared.FileManagement;
using Shared.Results;
using Shared.Tenancy;

internal sealed class DeleteFileCommandHandler(
    IFileStorage storage,
    ITenantContext tenantContext)
    : ICommandHandler<DeleteFileCommand, Unit>
{
    public async Task<Result<Unit>> HandleAsync(
        DeleteFileCommand command,
        CancellationToken cancellationToken)
    {
        if (tenantContext.IsEnabled && string.IsNullOrWhiteSpace(tenantContext.TenantId))
        {
            return Result.Failure<Unit>(FilesApplicationErrors.TenantRequired);
        }

        if (command.FileId == Guid.Empty)
        {
            return Result.Failure<Unit>(FilesApplicationErrors.FileIdInvalid);
        }

        FileStorageObjectKey key = FilesStorageKeys.For(command.FileId, tenantContext);
        bool deleted = await storage.DeleteAsync(key, cancellationToken).ConfigureAwait(false);

        return deleted
            ? Result.Success(Unit.Value)
            : Result.Failure<Unit>(FilesApplicationErrors.FileNotFound);
    }
}
