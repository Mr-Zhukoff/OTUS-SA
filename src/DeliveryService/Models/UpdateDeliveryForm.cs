using CoreLogic.Models;
using System.ComponentModel.DataAnnotations;

namespace DeliveryService.Models;

public class UpdateDeliveryForm
{
    [Required]
    public int UserId { get; set; }

    [Required]
    public int OrderId { get; set; }

    [Required]
    public string Title { get; set; }

    public string Description { get; set; }

    [Required]
    public string Address { get; set; }

    public DeliveryStatus Status { get; set; }

    public Delivery ToDelivery(int productId = 0)
    {
        return new Delivery
        {
            Id = productId,
            UserId = UserId,
            OrderId = OrderId,
            Title = Title,
            Description = Description,
            Address = Address,
            Status = Status
        };
    }
}
