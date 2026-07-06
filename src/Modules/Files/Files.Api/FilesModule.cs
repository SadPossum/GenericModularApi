namespace Files.Api;

using Files.Application;
using Files.Application.Commands;
using Files.Application.Queries;
using Files.Application.ReadModels;
using Files.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Hosting;
using Shared.Api.Modules;
using Shared.Api.Observability;
using Shared.Api.Results;
using Shared.Api.Tenancy;
using Shared.Cqrs;
using Shared.ModuleComposition;
using Shared.Results;

public sealed class FilesModule : IModule
{
    public string Name => FilesModuleMetadata.Name;

    public void AddServices(IHostApplicationBuilder builder)
    {
        builder.SelectModuleProfile(FilesProfiles.Default, "Files.Api");
        builder.Services.AddFilesApplication(builder.Configuration);
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/api/files")
            .WithModuleName(this.Name)
            .WithTags("Files")
            .RequireAuthorization();

        group.MapPost("/", async (
            IFormFile? file,
            IRequestDispatcher dispatcher,
            CancellationToken cancellationToken) =>
        {
            if (file is null)
            {
                return Result.Failure<FileUploadResponse>(FilesApplicationErrors.FileRequired)
                    .ToHttpResult(PublicErrorStatusCodes);
            }

            await using Stream stream = file.OpenReadStream();
            Result<FileUploadResponse> result = await dispatcher.SendAsync(
                new UploadFileCommand(stream, file.Length, file.ContentType, file.FileName),
                cancellationToken).ConfigureAwait(false);

            return result.IsFailure
                ? result.ToHttpResult(PublicErrorStatusCodes)
                : Results.Created(result.Value.DownloadPath, result.Value);
        })
            .RequireTenant()
            .RequireAuthorization()
            .DisableAntiforgery();

        group.MapGet("/{fileId:guid}", async (
            Guid fileId,
            IRequestDispatcher dispatcher,
            CancellationToken cancellationToken) =>
        {
            Result<FileDownload> result = await dispatcher.QueryAsync(
                new GetFileQuery(fileId),
                cancellationToken).ConfigureAwait(false);

            return result.IsFailure
                ? result.ToHttpResult(PublicErrorStatusCodes)
                : new FileDownloadHttpResult(result.Value);
        })
            .RequireTenant()
            .RequireAuthorization();

        group.MapDelete("/{fileId:guid}", async (
            Guid fileId,
            IRequestDispatcher dispatcher,
            CancellationToken cancellationToken) =>
        {
            Result<Unit> result = await dispatcher.SendAsync(
                new DeleteFileCommand(fileId),
                cancellationToken).ConfigureAwait(false);

            return result.IsSuccess ? Results.NoContent() : result.ToHttpResult(PublicErrorStatusCodes);
        })
            .RequireTenant()
            .RequireAuthorization();
    }

    private static readonly ApiErrorStatusCodeMap PublicErrorStatusCodes = ApiErrorStatusCodeMap.Create(
        new(FilesApplicationErrors.TenantRequired.Code, StatusCodes.Status400BadRequest),
        new(FilesApplicationErrors.FileRequired.Code, StatusCodes.Status400BadRequest),
        new(FilesApplicationErrors.FileEmpty.Code, StatusCodes.Status400BadRequest),
        new(FilesApplicationErrors.FileTooLarge.Code, StatusCodes.Status413PayloadTooLarge),
        new(FilesApplicationErrors.ContentTypeNotAllowed.Code, StatusCodes.Status415UnsupportedMediaType),
        new(FilesApplicationErrors.FileIdInvalid.Code, StatusCodes.Status400BadRequest),
        new(FilesApplicationErrors.FileNotFound.Code, StatusCodes.Status404NotFound));

    private sealed class FileDownloadHttpResult(FileDownload download) : IResult
    {
        public async Task ExecuteAsync(HttpContext httpContext)
        {
            httpContext.Response.ContentType = download.File.Properties.ContentType;
            httpContext.Response.ContentLength = download.File.Properties.ContentLength;
            await download.File.CopyToAsync(httpContext.Response.Body, httpContext.RequestAborted)
                .ConfigureAwait(false);
        }
    }
}
