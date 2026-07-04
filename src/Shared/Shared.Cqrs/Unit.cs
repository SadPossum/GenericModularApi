namespace Shared.Cqrs;

public readonly record struct Unit
{
    public static Unit Value => new();
}
