namespace Shared.Application.Tasks;

public enum TaskControlMessageStatus
{
    Unknown = 0,
    Pending = 1,
    Delivered = 2,
    Handled = 3,
    Failed = 4,
    Expired = 5
}
