namespace Shared.Messaging;

public interface IOutboxWriterRegistry
{
    IOutboxWriter GetRequired(string moduleName);
}
