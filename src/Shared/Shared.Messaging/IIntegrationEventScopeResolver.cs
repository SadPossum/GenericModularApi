namespace Shared.Messaging;

public interface IIntegrationEventScopeResolver
{
    string? ResolveScopeId(IIntegrationEvent integrationEvent);
}
