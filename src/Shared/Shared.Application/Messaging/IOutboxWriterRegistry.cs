namespace Shared.Application.Messaging;

public interface IOutboxWriterRegistry
{
    IOutboxWriter GetRequired(string moduleName);
}
