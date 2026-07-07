namespace Files.Application.Queries;

using Files.Application.ReadModels;
using Shared.AccessControl;
using Shared.Cqrs;

public sealed record GetFileQuery(Guid FileId, AccessSubject Subject) : IQuery<FileDownload>;
