namespace Files.Application.Queries;

using Files.Application.ReadModels;
using Shared.Cqrs;

public sealed record GetFileQuery(Guid FileId) : IQuery<FileDownload>;
