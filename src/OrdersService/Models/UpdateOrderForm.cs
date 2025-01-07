using CoreLogic.Models;
using System.ComponentModel.DataAnnotations;

namespace OrdersService.Models;

public class UpdateOrderForm
{
    [Required]
    public int UserId { get; set; }

    [Required]
    public int AccountId { get; set; }

    [Required]
    public string Title { get; set; }

    public string Description { get; set; }

    [Required]
    public decimal Amount { get; set; }

    public override string ToString()
    {
        return $"{UserId} {AccountId} {Title}";
    }
    public Order ToOrder(int orderId = 0)
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
