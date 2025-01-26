using CoreLogic.Models;
using System.ComponentModel.DataAnnotations;

namespace OrdersService.Models;

public class OrderForm
{
    [Required]
    public int UserId { get; set; }

    [Required]
    public int AccountId { get; set; }

    [Required]
    public string Title { get; set; }

    public string Description { get; set; }

    [Required]
    public List<OrderProductForm> ProductForms { get; set; }

    public override string ToString()
    {
        return $"{UserId} {AccountId} {Title}";
    }
    public Order ToOrder(int orderId = 0)
    {
        var order = new Order
        {
            Id = orderId,
            UserId = UserId,
            AccountId = AccountId,
            Title = Title,
            Description = Description,
            Products = new List<OrderProduct>()
        };
        decimal totalAmount = 0.0M;
        foreach (var item in ProductForms)
        {
            var orderProduct = new OrderProduct
            {
                ProductId = item.ProductId,
                OrderId = orderId,
                Title = item.Title,
                Price = item.Price,
                Quantity = item.Quantity,
                TotalPrice = item.Price * item.Quantity
            };
            order.Products.Add(orderProduct);
            totalAmount += orderProduct.TotalPrice;
        }
        order.Total = totalAmount;
        return order;
    }
}
