namespace Shared.Application.Cqrs;

public interface ITransactionalCommand<TResponse> : ICommand<TResponse> { }
