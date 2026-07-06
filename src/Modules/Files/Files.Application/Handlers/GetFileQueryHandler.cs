namespace Files.Application.Handlers;

using Files.Application.Queries;
using Files.Application.ReadModels;
using Shared.Cqrs;
using Shared.FileManagement;
using Shared.Results;
using Shared.Tenancy;

internal sealed class GetFileQueryHandler(
    IFileStorage storage,
    ITenantContext tenantContext)
    : IQueryHandler<GetFileQuery, FileDownload>
{
    public async Task<Result<FileDownload>> HandleAsync(
        GetFileQuery query,
        CancellationToken cancellationToken)
    {
        if (tenantContext.IsEnabled && string.IsNullOrWhiteSpace(tenantContext.TenantId))
        {
            return Result.Failure<FileDownload>(FilesApplicationErrors.TenantRequired);
        }

        if (query.FileId == Guid.Empty)
        {
            return Result.Failure<FileDownload>(FilesApplicationErrors.FileIdInvalid);
        }

        FileStorageObjectKey key = FilesStorageKeys.For(query.FileId, tenantContext);
        FileStorageReadResult? file = await storage.OpenReadAsync(key, cancellationToken).ConfigureAwait(false);

        return file is null
            ? Result.Failure<FileDownload>(FilesApplicationErrors.FileNotFound)
            : Result.Success(new FileDownload(file));
    }
}
