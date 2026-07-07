namespace Files.Application.Commands;

using Files.Contracts;
using Shared.AccessControl;
using Shared.Cqrs;

public sealed record UploadFileCommand(
    Stream Content,
    long ContentLength,
    string? ContentType,
    string? FileName,
    AccessSubject Subject)
    : ICommand<FileUploadResponse>;
