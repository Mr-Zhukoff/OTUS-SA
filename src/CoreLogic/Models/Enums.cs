namespace CoreLogic.Models;

public enum OrderStatus
{
    New = 0, 
    Processing = 1,
    Paid = 2,
    Reserved = 3,
    Sent = 4,
    Delivered = 5,
    Cancelled = 6,
    Error = 7
}

public enum DeliveryStatus
{
    New = 0,
    InProgress = 1,
    Delivered = 2,
    Cancelled = 3
}
