namespace Shared.Domain.Models;

public class AggregateRoot<T>(T id) : Entity<T>(id) { }
