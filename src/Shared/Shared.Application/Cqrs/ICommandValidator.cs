namespace Shared.Application.Cqrs;

public interface ICommandValidator<in TCommand>
{
    IEnumerable<string> Validate(TCommand command);
}
