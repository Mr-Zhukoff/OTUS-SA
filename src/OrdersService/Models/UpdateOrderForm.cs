using CoreLogic.Models;

namespace OrdersService.Models;

public class UpdateOrderForm
{
    public int UserId { get; set; }

    public int AccountId { get; set; }

    public string Title { get; set; }

    public string Description { get; set; }

    public decimal Amount { get; set; }

    public override string ToString()
    {
        return $"{UserId} {AccountId} {Title}";
    }
    public Order ToOrder(int orderId)
    {
        return new Order
        {
            Id = orderId,
            UserId = UserId,
            AccountId = AccountId,
            Title = Title,
            Description = Description,
            Amount = Amount
        };
    }
}
