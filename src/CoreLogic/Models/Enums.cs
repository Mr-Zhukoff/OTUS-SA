namespace CoreLogic.Models;

public enum OrderStatus
{
    New = 0, 
    Processing = 1,
    Paid = 2,
    Sent = 3,
    Delivered = 4,
    Cancelled = 5
}

public enum DeliveryStatus
{
    New = 0,
    InProgress = 1,
    Delivered = 2,
    Cancelled = 3
}
