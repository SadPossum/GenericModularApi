namespace Files.Application.Commands;

using Shared.Cqrs;

public sealed record DeleteFileCommand(Guid FileId) : ICommand<Unit>;
