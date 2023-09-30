namespace Shared.Domain.Models;

public class Entity<T>(T id)
{
    public T Id { get; private set; } = id;
}
