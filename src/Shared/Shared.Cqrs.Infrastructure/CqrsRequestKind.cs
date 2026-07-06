namespace Shared.Cqrs.Infrastructure;

public enum CqrsRequestKind
{
    Unknown = 0,
    Command = 1,
    Query = 2
}
