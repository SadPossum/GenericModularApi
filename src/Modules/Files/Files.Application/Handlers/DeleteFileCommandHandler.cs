namespace Files.Application.Handlers;

using Files.Application.Commands;
using Files.Application.Visibility;
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
        Result<Unit> access = FilesAccess.EnsureUserSubject(command.Subject, tenantContext);
        if (access.IsFailure)
        {
            return access;
        }

        if (command.FileId == Guid.Empty)
        {
            return Result.Failure<Unit>(FilesApplicationErrors.FileIdInvalid);
        }

        FileStorageObjectKey key = FilesStorageKeys.For(command.FileId, command.Subject, tenantContext);
        bool deleted = await storage.DeleteAsync(key, cancellationToken).ConfigureAwait(false);

        return deleted
            ? Result.Success(Unit.Value)
            : Result.Failure<Unit>(FilesApplicationErrors.FileNotFound);
    }
}
