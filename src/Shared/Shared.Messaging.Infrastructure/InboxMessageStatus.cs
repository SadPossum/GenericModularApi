namespace Shared.Messaging.Infrastructure;

public enum InboxMessageStatus
{
    Pending = 0,
    Processing = 1,
    Processed = 2,
    Failed = 3
}
