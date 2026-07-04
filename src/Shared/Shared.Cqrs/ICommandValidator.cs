namespace Shared.Cqrs;

public interface ICommandValidator<in TCommand>
{
    IEnumerable<string> Validate(TCommand command);
}
