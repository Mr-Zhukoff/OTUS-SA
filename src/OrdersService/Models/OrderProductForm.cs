using System.ComponentModel.DataAnnotations;

namespace OrdersService.Models;

public class OrderProductForm
{
    [Required]
    public int ProductId { get; set; }

    [Required]
    public string Title { get; set; }

    [Required]
    public int Quantity { get; set; }

    [Required]
    public decimal Price { get; set; }
}
