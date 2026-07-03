namespace Shared.Application.Messaging;

public enum InboxProcessStatus
{
    Unknown = 0,
    Processed = 1,
    Duplicate = 2,
    Failed = 3
}
