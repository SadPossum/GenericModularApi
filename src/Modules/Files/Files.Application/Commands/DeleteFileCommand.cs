namespace Files.Application.Commands;

using Shared.AccessControl;
using Shared.Cqrs;

public sealed record DeleteFileCommand(Guid FileId, AccessSubject Subject) : ICommand<Unit>;
